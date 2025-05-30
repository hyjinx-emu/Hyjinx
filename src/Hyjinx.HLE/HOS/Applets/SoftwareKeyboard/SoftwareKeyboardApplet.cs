using Hyjinx.Common;
using Hyjinx.Common.Configuration.Hid;
using Hyjinx.HLE.HOS.Applets.SoftwareKeyboard;
using Hyjinx.HLE.HOS.Services.Am.AppletAE;
using Hyjinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad;
using Hyjinx.HLE.UI;
using Hyjinx.HLE.UI.Input;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Hyjinx.HLE.HOS.Applets;

internal partial class SoftwareKeyboardApplet : IApplet
{
    private const string DefaultInputText = "Hyjinx";

    private const int StandardBufferSize = 0x7D8;
    private const int InteractiveBufferSize = 0x7D4;
    private const int MaxUserWords = 0x1388;
    private const int MaxUiTextSize = 100;

    private const Key CycleInputModesKey = Key.F6;

    private readonly Switch _device;

    private SoftwareKeyboardState _foregroundState = SoftwareKeyboardState.Uninitialized;
    private volatile InlineKeyboardState _backgroundState = InlineKeyboardState.Uninitialized;

    private bool _isBackground = false;

    private AppletSession _normalSession;
    private AppletSession _interactiveSession;

    // Configuration for foreground mode.
    private SoftwareKeyboardConfig _keyboardForegroundConfig;

    // Configuration for background (inline) mode.
#pragma warning disable IDE0052 // Remove unread private member
    private SoftwareKeyboardInitialize _keyboardBackgroundInitialize;
    private SoftwareKeyboardCustomizeDic _keyboardBackgroundDic;
    private SoftwareKeyboardDictSet _keyboardBackgroundDictSet;
#pragma warning restore IDE0052
    private SoftwareKeyboardUserWord[] _keyboardBackgroundUserWords;

    private byte[] _transferMemory;

    private string _textValue = "";
    private int _cursorBegin = 0;
    private Encoding _encoding = Encoding.Unicode;
    private KeyboardResult _lastResult = KeyboardResult.NotSet;

    private IDynamicTextInputHandler _dynamicTextInputHandler = null;
    private SoftwareKeyboardRenderer _keyboardRenderer = null;
    private NpadReader _npads = null;
    private bool _canAcceptController = false;
    private KeyboardInputMode _inputMode = KeyboardInputMode.ControllerAndKeyboard;

    private static readonly ILogger<SoftwareKeyboardApplet> _logger = Logger.DefaultLoggerFactory.CreateLogger<SoftwareKeyboardApplet>();
    private readonly object _lock = new();

    public event EventHandler AppletStateChanged;

    public SoftwareKeyboardApplet(Horizon system)
    {
        _device = system.Device;
    }

    public ResultCode Start(AppletSession normalSession, AppletSession interactiveSession)
    {
        lock (_lock)
        {
            _normalSession = normalSession;
            _interactiveSession = interactiveSession;

            _interactiveSession.DataAvailable += OnInteractiveData;

            var launchParams = _normalSession.Pop();
            var keyboardConfig = _normalSession.Pop();

            _isBackground = keyboardConfig.Length == Unsafe.SizeOf<SoftwareKeyboardInitialize>();

            if (_isBackground)
            {
                // Initialize the keyboard applet in background mode.

                _keyboardBackgroundInitialize = MemoryMarshal.Read<SoftwareKeyboardInitialize>(keyboardConfig);
                _backgroundState = InlineKeyboardState.Uninitialized;

                if (_device.UIHandler == null)
                {
                    LogGuiHandlerIsNotSetAppletWillNotWork();
                }
                else
                {
                    // Create a text handler that converts keyboard strokes to strings.
                    _dynamicTextInputHandler = _device.UIHandler.CreateDynamicTextInputHandler();
                    _dynamicTextInputHandler.TextChangedEvent += HandleTextChangedEvent;
                    _dynamicTextInputHandler.KeyPressedEvent += HandleKeyPressedEvent;

                    _npads = new NpadReader(_device);
                    _npads.NpadButtonDownEvent += HandleNpadButtonDownEvent;
                    _npads.NpadButtonUpEvent += HandleNpadButtonUpEvent;

                    _keyboardRenderer = new SoftwareKeyboardRenderer(_device.UIHandler.HostUITheme);
                }

                return ResultCode.Success;
            }
            else
            {
                // Initialize the keyboard applet in foreground mode.

                if (keyboardConfig.Length < Marshal.SizeOf<SoftwareKeyboardConfig>())
                {
                    LogSoftwareKeyboardConfigSizeMismatch(Marshal.SizeOf<SoftwareKeyboardConfig>(), keyboardConfig.Length);
                }
                else
                {
                    _keyboardForegroundConfig = ReadStruct<SoftwareKeyboardConfig>(keyboardConfig);
                }

                if (!_normalSession.TryPop(out _transferMemory))
                {
                    LogSwKbdTransferMemoryIsNull();
                }

                if (_keyboardForegroundConfig.UseUtf8)
                {
                    _encoding = Encoding.UTF8;
                }

                _foregroundState = SoftwareKeyboardState.Ready;

                ExecuteForegroundKeyboard();

                return ResultCode.Success;
            }
        }
    }

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "GUI Handler is not set, software keyboard applet will not work properly.")]
    private partial void LogGuiHandlerIsNotSetAppletWillNotWork();

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "SwKbd Transfer Memory is null.")]
    private partial void LogSwKbdTransferMemoryIsNull();

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "SoftwareKeyboardConfig size mismatch. Expected {expected:X}, got {actual:X}.")]
    private partial void LogSoftwareKeyboardConfigSizeMismatch(int expected, int actual);

    public ResultCode GetResult()
    {
        return ResultCode.Success;
    }

    private bool IsKeyboardActive()
    {
        return _backgroundState >= InlineKeyboardState.Appearing && _backgroundState < InlineKeyboardState.Disappearing;
    }

    private bool InputModeControllerEnabled()
    {
        return _inputMode == KeyboardInputMode.ControllerAndKeyboard ||
               _inputMode == KeyboardInputMode.ControllerOnly;
    }

    private bool InputModeTypingEnabled()
    {
        return _inputMode == KeyboardInputMode.ControllerAndKeyboard ||
               _inputMode == KeyboardInputMode.KeyboardOnly;
    }

    private void AdvanceInputMode()
    {
        _inputMode = (KeyboardInputMode)((int)(_inputMode + 1) % (int)KeyboardInputMode.Count);
    }

    public bool DrawTo(RenderingSurfaceInfo surfaceInfo, IVirtualMemoryManager destination, ulong position)
    {
        _npads?.Update();

        _keyboardRenderer?.SetSurfaceInfo(surfaceInfo);

        return _keyboardRenderer?.DrawTo(destination, position) ?? false;
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.Application, EventName = nameof(LogClass.Application),
        Message = "GUI Handler is not set, falling back to default!")]
    private partial void LogGuiHandlerIsNotSetFallingBackToDefault();

    private void ExecuteForegroundKeyboard()
    {
        string initialText = null;

        // Initial Text is always encoded as a UTF-16 string in the work buffer (passed as transfer memory)
        // InitialStringOffset points to the memory offset and InitialStringLength is the number of UTF-16 characters
        if (_transferMemory != null && _keyboardForegroundConfig.InitialStringLength > 0)
        {
            initialText = Encoding.Unicode.GetString(_transferMemory, _keyboardForegroundConfig.InitialStringOffset,
                2 * _keyboardForegroundConfig.InitialStringLength);
        }

        // If the max string length is 0, we set it to a large default
        // length.
        if (_keyboardForegroundConfig.StringLengthMax == 0)
        {
            _keyboardForegroundConfig.StringLengthMax = 100;
        }

        if (_device.UIHandler == null)
        {
            LogGuiHandlerIsNotSetFallingBackToDefault();

            _textValue = DefaultInputText;
            _lastResult = KeyboardResult.Accept;
        }
        else
        {
            // Call the configured GUI handler to get user's input.
            var args = new SoftwareKeyboardUIArgs
            {
                KeyboardMode = _keyboardForegroundConfig.Mode,
                HeaderText = StripUnicodeControlCodes(_keyboardForegroundConfig.HeaderText),
                SubtitleText = StripUnicodeControlCodes(_keyboardForegroundConfig.SubtitleText),
                GuideText = StripUnicodeControlCodes(_keyboardForegroundConfig.GuideText),
                SubmitText = (!string.IsNullOrWhiteSpace(_keyboardForegroundConfig.SubmitText) ?
                _keyboardForegroundConfig.SubmitText : "OK"),
                StringLengthMin = _keyboardForegroundConfig.StringLengthMin,
                StringLengthMax = _keyboardForegroundConfig.StringLengthMax,
                InitialText = initialText,
            };

            _lastResult = _device.UIHandler.DisplayInputDialog(args, out _textValue) ? KeyboardResult.Accept : KeyboardResult.Cancel;
            _textValue ??= initialText ?? DefaultInputText;
        }

        // If the game requests a string with a minimum length less
        // than our default text, repeat our default text until we meet
        // the minimum length requirement.
        // This should always be done before the text truncation step.
        while (_textValue.Length < _keyboardForegroundConfig.StringLengthMin)
        {
            _textValue = String.Join(" ", _textValue, _textValue);
        }

        // If our default text is longer than the allowed length,
        // we truncate it.
        if (_textValue.Length > _keyboardForegroundConfig.StringLengthMax)
        {
            _textValue = _textValue[.._keyboardForegroundConfig.StringLengthMax];
        }

        // Does the application want to validate the text itself?
        if (_keyboardForegroundConfig.CheckText)
        {
            // The application needs to validate the response, so we
            // submit it to the interactive output buffer, and poll it
            // for validation. Once validated, the application will submit
            // back a validation status, which is handled in OnInteractiveDataPushIn.
            _foregroundState = SoftwareKeyboardState.ValidationPending;

            PushForegroundResponse(true);
        }
        else
        {
            // If the application doesn't need to validate the response,
            // we push the data to the non-interactive output buffer
            // and poll it for completion.
            _foregroundState = SoftwareKeyboardState.Complete;

            PushForegroundResponse(false);

            AppletStateChanged?.Invoke(this, null);
        }
    }

    private void OnInteractiveData(object sender, EventArgs e)
    {
        // Obtain the validation status response.
        var data = _interactiveSession.Pop();

        if (_isBackground)
        {
            lock (_lock)
            {
                OnBackgroundInteractiveData(data);
            }
        }
        else
        {
            OnForegroundInteractiveData(data);
        }
    }

    private void OnForegroundInteractiveData(byte[] data)
    {
        if (_foregroundState == SoftwareKeyboardState.ValidationPending)
        {
            // TODO(jduncantor):
            // If application rejects our "attempt", submit another attempt,
            // and put the applet back in PendingValidation state.

            // For now we assume success, so we push the final result
            // to the standard output buffer and carry on our merry way.
            PushForegroundResponse(false);

            AppletStateChanged?.Invoke(this, null);

            _foregroundState = SoftwareKeyboardState.Complete;
        }
        else if (_foregroundState == SoftwareKeyboardState.Complete)
        {
            // If we have already completed, we push the result text
            // back on the output buffer and poll the application.
            PushForegroundResponse(false);

            AppletStateChanged?.Invoke(this, null);
        }
        else
        {
            // We shouldn't be able to get here through standard swkbd execution.
            throw new InvalidOperationException("Software Keyboard is in an invalid state.");
        }
    }

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Keyboard received command {request} in state {state}")]
    private partial void LogKeyboardReceivedCommand(InlineKeyboardRequest request, InlineKeyboardState state);

    private void OnBackgroundInteractiveData(byte[] data)
    {
        // WARNING: Only invoke applet state changes after an explicit finalization
        // request from the game, this is because the inline keyboard is expected to
        // keep running in the background sending data by itself.

        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);

        var request = (InlineKeyboardRequest)reader.ReadUInt32();

        long remaining;

        LogKeyboardReceivedCommand(request, _backgroundState);

        switch (request)
        {
            case InlineKeyboardRequest.UseChangedStringV2:
                // Logger.Stub?.Print(LogClass.ServiceAm, "Inline keyboard request UseChangedStringV2");
                break;
            case InlineKeyboardRequest.UseMovedCursorV2:
                // Logger.Stub?.Print(LogClass.ServiceAm, "Inline keyboard request UseMovedCursorV2");
                break;
            case InlineKeyboardRequest.SetUserWordInfo:
                // Read the user word info data.
                remaining = stream.Length - stream.Position;
                if (remaining < sizeof(int))
                {
                    LogInvalidKeyboardUserWordInfo(remaining);
                }
                else
                {
                    int wordsCount = reader.ReadInt32();
                    int wordSize = Unsafe.SizeOf<SoftwareKeyboardUserWord>();
                    remaining = stream.Length - stream.Position;

                    if (wordsCount > MaxUserWords)
                    {
                        LogExceededMaximumWords(wordsCount, MaxUserWords);
                    }
                    else if (wordsCount * wordSize != remaining)
                    {
                        LogInvalidKeyboardUserWordInfo(remaining, wordsCount);
                    }
                    else
                    {
                        _keyboardBackgroundUserWords = new SoftwareKeyboardUserWord[wordsCount];

                        for (int word = 0; word < wordsCount; word++)
                        {
                            _keyboardBackgroundUserWords[word] = reader.ReadStruct<SoftwareKeyboardUserWord>();
                        }
                    }
                }
                _interactiveSession.Push(InlineResponses.ReleasedUserWordInfo(_backgroundState));
                break;
            case InlineKeyboardRequest.SetCustomizeDic:
                // Read the custom dic data.
                remaining = stream.Length - stream.Position;
                if (remaining != Unsafe.SizeOf<SoftwareKeyboardCustomizeDic>())
                {
                    LogInvalidKeyboardCustomizeDic(remaining);
                }
                else
                {
                    _keyboardBackgroundDic = reader.ReadStruct<SoftwareKeyboardCustomizeDic>();
                }
                break;
            case InlineKeyboardRequest.SetCustomizedDictionaries:
                // Read the custom dictionaries data.
                remaining = stream.Length - stream.Position;
                if (remaining != Unsafe.SizeOf<SoftwareKeyboardDictSet>())
                {
                    LogInvalidKeyboardDictSet(remaining);
                }
                else
                {
                    _keyboardBackgroundDictSet = reader.ReadStruct<SoftwareKeyboardDictSet>();
                }
                break;
            case InlineKeyboardRequest.Calc:
                // The Calc request is used to communicate configuration changes and commands to the keyboard.
                // Fields in the Calc struct and operations are masked by the Flags field.

                // Read the Calc data.
                SoftwareKeyboardCalcEx newCalc;
                remaining = stream.Length - stream.Position;
                if (remaining == Marshal.SizeOf<SoftwareKeyboardCalc>())
                {
                    var keyboardCalcData = reader.ReadBytes((int)remaining);
                    var keyboardCalc = ReadStruct<SoftwareKeyboardCalc>(keyboardCalcData);

                    newCalc = keyboardCalc.ToExtended();
                }
                else if (remaining == Marshal.SizeOf<SoftwareKeyboardCalcEx>() || remaining == SoftwareKeyboardCalcEx.AlternativeSize)
                {
                    var keyboardCalcData = reader.ReadBytes((int)remaining);

                    newCalc = ReadStruct<SoftwareKeyboardCalcEx>(keyboardCalcData);
                }
                else
                {
                    LogInvalidSoftwareKeyboardCalc(remaining);

                    newCalc = new SoftwareKeyboardCalcEx();
                }

                // Process each individual operation specified in the flags.

                bool updateText = false;

                if ((newCalc.Flags & KeyboardCalcFlags.Initialize) != 0)
                {
                    _interactiveSession.Push(InlineResponses.FinishedInitialize(_backgroundState));

                    _backgroundState = InlineKeyboardState.Initialized;
                }

                if ((newCalc.Flags & KeyboardCalcFlags.SetCursorPos) != 0)
                {
                    _cursorBegin = newCalc.CursorPos;
                    updateText = true;

                    LogCursorPositionChanged(_cursorBegin);
                }

                if ((newCalc.Flags & KeyboardCalcFlags.SetInputText) != 0)
                {
                    _textValue = newCalc.InputText;
                    updateText = true;

                    LogInputTextChanged(_textValue!);
                }

                if ((newCalc.Flags & KeyboardCalcFlags.SetUtf8Mode) != 0)
                {
                    _encoding = newCalc.UseUtf8 ? Encoding.UTF8 : Encoding.Default;

                    LogEncodingChanged(_encoding);
                }

                if (updateText)
                {
                    _dynamicTextInputHandler.SetText(_textValue, _cursorBegin);
                    _keyboardRenderer.UpdateTextState(_textValue, _cursorBegin, _cursorBegin, null, null);
                }

                if ((newCalc.Flags & KeyboardCalcFlags.MustShow) != 0)
                {
                    ActivateFrontend();

                    _backgroundState = InlineKeyboardState.Shown;

                    PushChangedString(_textValue, (uint)_cursorBegin, _backgroundState);
                }

                // Send the response to the Calc
                _interactiveSession.Push(InlineResponses.Default(_backgroundState));
                break;
            case InlineKeyboardRequest.Finalize:
                // Destroy the frontend.
                DestroyFrontend();
                // The calling application wants to close the keyboard applet and will wait for a state change.
                _backgroundState = InlineKeyboardState.Uninitialized;
                AppletStateChanged?.Invoke(this, null);
                break;
            default:
                // We shouldn't be able to get here through standard swkbd execution.
                LogInvalidKeyboardRequest(request, _backgroundState);

                _interactiveSession.Push(InlineResponses.Default(_backgroundState));
                break;
        }
    }

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Received invalid software keyboard user word info of {remaining} bytes")]
    private partial void LogInvalidKeyboardUserWordInfo(long remaining);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Received {wordsCount} user words but the maximum is {maxUserWords}")]
    private partial void LogExceededMaximumWords(int wordsCount, int maxUserWords);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Received invalid software keyboard user word info data of {remaining} bytes for {wordsCount} words.")]
    private partial void LogInvalidKeyboardUserWordInfo(long remaining, int wordsCount);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Received invalid software customize dic of {remaining} bytes")]
    private partial void LogInvalidKeyboardCustomizeDic(long remaining);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Received invalid software keyboard DictSet of {remaining} bytes")]
    private partial void LogInvalidKeyboardDictSet(long remaining);

    [LoggerMessage(LogLevel.Warning,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Invalid software keyboard request {request} during state {state}")]
    private partial void LogInvalidKeyboardRequest(InlineKeyboardRequest request, InlineKeyboardState state);

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Cursor position set to {pos}")]
    private partial void LogCursorPositionChanged(int pos);

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Input text set to '{text}'.")]
    private partial void LogInputTextChanged(string text);

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Encoding set to {encoding}.")]
    private partial void LogEncodingChanged(Encoding encoding);

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Activating software keyboard frontend.")]
    private partial void LogActivatingSoftwareKeyboard();

    [LoggerMessage(LogLevel.Error,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Received invalid Software Keyboard Calc of {remaining} bytes.")]
    private partial void LogInvalidSoftwareKeyboardCalc(long remaining);

    private void ActivateFrontend()
    {
        LogActivatingSoftwareKeyboard();

        _inputMode = KeyboardInputMode.ControllerAndKeyboard;

        _npads.Update(true);

        NpadButton buttons = _npads.GetCurrentButtonsOfAllNpads();

        // Block the input if the current accept key is pressed so the applet won't be instantly closed.
        _canAcceptController = (buttons & NpadButton.A) == 0;

        _dynamicTextInputHandler.TextProcessingEnabled = true;

        _keyboardRenderer.UpdateCommandState(null, null, true);
        _keyboardRenderer.UpdateTextState(null, null, null, null, true);
    }

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Deactivating software keyboard frontend.")]
    private partial void LogDeactivatingSoftwareKeyboard();

    private void DeactivateFrontend()
    {
        LogDeactivatingSoftwareKeyboard();

        _inputMode = KeyboardInputMode.ControllerAndKeyboard;
        _canAcceptController = false;

        _dynamicTextInputHandler.TextProcessingEnabled = false;
        _dynamicTextInputHandler.SetText(_textValue, _cursorBegin);
    }

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Destroying software keyboard frontend.")]
    private partial void LogDestroyingSoftwareKeyboard();

    private void DestroyFrontend()
    {
        LogDestroyingSoftwareKeyboard();

        _keyboardRenderer?.Dispose();
        _keyboardRenderer = null;

        if (_dynamicTextInputHandler != null)
        {
            _dynamicTextInputHandler.TextChangedEvent -= HandleTextChangedEvent;
            _dynamicTextInputHandler.KeyPressedEvent -= HandleKeyPressedEvent;
            _dynamicTextInputHandler.Dispose();
            _dynamicTextInputHandler = null;
        }

        if (_npads != null)
        {
            _npads.NpadButtonDownEvent -= HandleNpadButtonDownEvent;
            _npads.NpadButtonUpEvent -= HandleNpadButtonUpEvent;
            _npads = null;
        }
    }

    private bool HandleKeyPressedEvent(Key key)
    {
        if (key == CycleInputModesKey)
        {
            lock (_lock)
            {
                if (IsKeyboardActive())
                {
                    AdvanceInputMode();

                    bool typingEnabled = InputModeTypingEnabled();
                    bool controllerEnabled = InputModeControllerEnabled();

                    _dynamicTextInputHandler.TextProcessingEnabled = typingEnabled;

                    _keyboardRenderer.UpdateTextState(null, null, null, null, typingEnabled);
                    _keyboardRenderer.UpdateCommandState(null, null, controllerEnabled);
                }
            }
        }

        return true;
    }

    private void HandleTextChangedEvent(string text, int cursorBegin, int cursorEnd, bool overwriteMode)
    {
        lock (_lock)
        {
            // Text processing should not run with typing disabled.
            Debug.Assert(InputModeTypingEnabled());

            if (text.Length > MaxUiTextSize)
            {
                // Limit the text size and change it back.
                text = text[..MaxUiTextSize];
                cursorBegin = Math.Min(cursorBegin, MaxUiTextSize);
                cursorEnd = Math.Min(cursorEnd, MaxUiTextSize);

                _dynamicTextInputHandler.SetText(text, cursorBegin, cursorEnd);
            }

            _textValue = text;
            _cursorBegin = cursorBegin;
            _keyboardRenderer.UpdateTextState(text, cursorBegin, cursorEnd, overwriteMode, null);

            PushUpdatedState(text, cursorBegin, KeyboardResult.NotSet);
        }
    }

    private void HandleNpadButtonDownEvent(int npadIndex, NpadButton button)
    {
        lock (_lock)
        {
            if (!IsKeyboardActive())
            {
                return;
            }

            switch (button)
            {
                case NpadButton.A:
                    _keyboardRenderer.UpdateCommandState(_canAcceptController, null, null);
                    break;
                case NpadButton.B:
                    _keyboardRenderer.UpdateCommandState(null, _canAcceptController, null);
                    break;
            }
        }
    }

    private void HandleNpadButtonUpEvent(int npadIndex, NpadButton button)
    {
        lock (_lock)
        {
            KeyboardResult result = KeyboardResult.NotSet;

            switch (button)
            {
                case NpadButton.A:
                    result = KeyboardResult.Accept;
                    _keyboardRenderer.UpdateCommandState(false, null, null);
                    break;
                case NpadButton.B:
                    result = KeyboardResult.Cancel;
                    _keyboardRenderer.UpdateCommandState(null, false, null);
                    break;
            }

            if (IsKeyboardActive())
            {
                if (!_canAcceptController)
                {
                    _canAcceptController = true;
                }
                else if (InputModeControllerEnabled())
                {
                    PushUpdatedState(_textValue, _cursorBegin, result);
                }
            }
        }
    }

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Updating keyboard text to {text} and cursor position to {pos}")]
    private partial void LogSettingKeyboardAndCursor(string text, int pos);

    private void PushUpdatedState(string text, int cursorBegin, KeyboardResult result)
    {
        _lastResult = result;
        _textValue = text;

        bool cancel = result == KeyboardResult.Cancel;
        bool accept = result == KeyboardResult.Accept;

        if (!IsKeyboardActive())
        {
            // Keyboard is not active.

            return;
        }

        if (accept == false && cancel == false)
        {
            LogSettingKeyboardAndCursor(text, _cursorBegin);

            PushChangedString(text, (uint)cursorBegin, _backgroundState);
        }
        else
        {
            // Disable the frontend.
            DeactivateFrontend();

            // The 'Complete' state indicates the Calc request has been fulfilled by the applet.
            _backgroundState = InlineKeyboardState.Disappearing;

            if (accept)
            {
                LogSendingKeyboardOk(text);

                DecidedEnter(text, _backgroundState);
            }
            else if (cancel)
            {
                LogSendingKeyboardCancel();

                DecidedCancel(_backgroundState);
            }

            _interactiveSession.Push(InlineResponses.Default(_backgroundState));

            LogResettingKeyboardState(_backgroundState);

            // Set the state of the applet to 'Initialized' as it is the only known state so far
            // that does not soft-lock the keyboard after use.

            _backgroundState = InlineKeyboardState.Initialized;

            _interactiveSession.Push(InlineResponses.Default(_backgroundState));
        }
    }

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Sending keyboard OK with text '{text}'.")]
    private partial void LogSendingKeyboardOk(string text);

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Sending keyboard cancel.")]
    private partial void LogSendingKeyboardCancel();

    [LoggerMessage(LogLevel.Debug,
        EventId = (int)LogClass.ServiceAm, EventName = nameof(LogClass.ServiceAm),
        Message = "Resetting state of the keyboard to '{state}'.")]
    private partial void LogResettingKeyboardState(InlineKeyboardState state);

    private void PushChangedString(string text, uint cursor, InlineKeyboardState state)
    {
        // TODO (Caian): The *V2 methods are not supported because the applications that request
        // them do not seem to accept them. The regular methods seem to work just fine in all cases.

        if (_encoding == Encoding.UTF8)
        {
            _interactiveSession.Push(InlineResponses.ChangedStringUtf8(text, cursor, state));
        }
        else
        {
            _interactiveSession.Push(InlineResponses.ChangedString(text, cursor, state));
        }
    }

    private void DecidedEnter(string text, InlineKeyboardState state)
    {
        if (_encoding == Encoding.UTF8)
        {
            _interactiveSession.Push(InlineResponses.DecidedEnterUtf8(text, state));
        }
        else
        {
            _interactiveSession.Push(InlineResponses.DecidedEnter(text, state));
        }
    }

    private void DecidedCancel(InlineKeyboardState state)
    {
        _interactiveSession.Push(InlineResponses.DecidedCancel(state));
    }

    private void PushForegroundResponse(bool interactive)
    {
        int bufferSize = interactive ? InteractiveBufferSize : StandardBufferSize;

        using MemoryStream stream = new(new byte[bufferSize]);
        using BinaryWriter writer = new(stream);
        byte[] output = _encoding.GetBytes(_textValue);

        if (!interactive)
        {
            // Result Code.
            writer.Write(_lastResult == KeyboardResult.Accept ? 0U : 1U);
        }
        else
        {
            // In interactive mode, we write the length of the text as a long, rather than
            // a result code. This field is inclusive of the 64-bit size.
            writer.Write((long)output.Length + 8);
        }

        writer.Write(output);

        if (!interactive)
        {
            _normalSession.Push(stream.ToArray());
        }
        else
        {
            _interactiveSession.Push(stream.ToArray());
        }
    }

    /// <summary>
    /// Removes all Unicode control code characters from the input string.
    /// This includes CR/LF, tabs, null characters, escape characters,
    /// and special control codes which are used for formatting by the real keyboard applet.
    /// </summary>
    /// <remarks>
    /// Some games send special control codes (such as 0x13 "Device Control 3") as part of the string.
    /// Future implementations of the emulated keyboard applet will need to handle these as well.
    /// </remarks>
    /// <param name="input">The input string to sanitize (may be null).</param>
    /// <returns>The sanitized string.</returns>
    internal static string StripUnicodeControlCodes(string input)
    {
        if (input is null)
        {
            return null;
        }

        if (input.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new(capacity: input.Length);
        foreach (char c in input)
        {
            if (!char.IsControl(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static T ReadStruct<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(byte[] data)
        where T : struct
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

        try
        {
            return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }
}
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.FsSrv.Impl;
using LibHac.Ncm;
using LibHac.Util;
using System;
using NUnit.Framework;

namespace LibHac.Tests.FsSrv;

public class ProgramIndexMapInfoTests
{
    [Test]
    public void GetProgramIndexForAccessLog_IsMultiProgram_ReturnsCorrectIndex()
    {
        const int count = 7;

        Horizon hos = HorizonFactory.CreateBasicHorizon();

        var programs = new HorizonClient[count];

        programs[0] = hos.CreateHorizonClient(new ProgramLocation(new ProgramId(1), StorageId.BuiltInSystem),
            AccessControlBits.Bits.RegisterProgramIndexMapInfo);

        for (int i = 1; i < programs.Length; i++)
        {
            programs[i] =
                hos.CreateHorizonClient(new ProgramLocation(new ProgramId((ulong)(i + 1)), StorageId.BuiltInSystem),
                    AccessControlBits.Bits.None);
        }

        var map = new ProgramIndexMapInfo[count];

        for (int i = 0; i < map.Length; i++)
        {
            map[i].MainProgramId = new Ncm.ApplicationId(1);
            map[i].ProgramId = new ProgramId((ulong)(i + 1));
            map[i].ProgramIndex = (byte)i;
        }

        Result registerResult = programs[0].Fs.RegisterProgramIndexMapInfo(map);
        Assert.That(registerResult.IsSuccess(), Is.True, registerResult.ToString());

        for (int i = 0; i < programs.Length; i++)
        {
            using SharedRef<LibHac.FsSrv.Sf.IFileSystemProxy> fsProxy =
                programs[i].Fs.Impl.GetFileSystemProxyServiceObject();
            Result result = fsProxy.Get.GetProgramIndexForAccessLog(out int programIndex, out int programCount);
            Assert.That(result.IsSuccess(), Is.True, result.ToString());

            Assert.That(programIndex, Is.EqualTo(i));
            Assert.That(programCount, Is.EqualTo(count));
        }
    }

    private ProgramIndexMapInfoManager CreatePopulatedManager(int count, Func<int, long> idCreator)
    {
        var manager = new ProgramIndexMapInfoManager();

        var map = new ProgramIndexMapInfo[count];

        for (int i = 0; i < map.Length; i++)
        {
            map[i].MainProgramId = new Ncm.ApplicationId((ulong)idCreator(0));
            map[i].ProgramId = new ProgramId((ulong)idCreator(i));
            map[i].ProgramIndex = (byte)i;
        }

        Result result = manager.Reset(map);
        Assert.That(result.IsSuccess(), Is.True, result.ToString());

        return manager;
    }

    [Test]
    public void Get_IdDoesNotExist_ReturnsNull()
    {
        const int count = 5;

        ProgramIndexMapInfoManager manager = CreatePopulatedManager(count, x => x + 3);

        Assert.That(manager.Get(new ProgramId(0)).HasValue, Is.False);
        Assert.That(manager.Get(new ProgramId(2)).HasValue, Is.False);
        Assert.That(manager.Get(new ProgramId(8)).HasValue, Is.False);
        Assert.That(manager.Get(new ProgramId(9001)).HasValue, Is.False);
    }

    [Test]
    public void Get_IdExists_ReturnsCorrectMapInfo()
    {
        const int count = 5;

        ProgramIndexMapInfoManager manager = CreatePopulatedManager(count, x => x + 1);

        // ReSharper disable PossibleInvalidOperationException
        Optional<ProgramIndexMapInfo> map = manager.Get(new ProgramId(1));
        Assert.That(map.HasValue, Is.True);
        Assert.That(map.Value.MainProgramId, Is.EqualTo(new Ncm.ApplicationId(1)));
        Assert.That(map.Value.ProgramId, Is.EqualTo(new ProgramId(1)));
        Assert.That(map.Value.ProgramIndex, Is.EqualTo(0));

        map = manager.Get(new ProgramId(4));
        Assert.That(map.HasValue, Is.True);
        Assert.That(map.Value.MainProgramId, Is.EqualTo(new Ncm.ApplicationId(1)));
        Assert.That(map.Value.ProgramId, Is.EqualTo(new ProgramId(4)));
        Assert.That(map.Value.ProgramIndex, Is.EqualTo(3));
        // ReSharper restore PossibleInvalidOperationException
    }

    [Test]
    public void GetProgramId_WithIndex_ReturnsProgramIdOfSpecifiedIndex()
    {
        const int count = 5;

        ProgramIndexMapInfoManager manager = CreatePopulatedManager(count, x => (x + 1) * 5);

        // Check that the program ID can be retrieved using any program in the set
        Assert.That(manager.GetProgramId(new ProgramId(5), 3), Is.EqualTo(new ProgramId(20)));
        Assert.That(manager.GetProgramId(new ProgramId(10), 3), Is.EqualTo(new ProgramId(20)));
        Assert.That(manager.GetProgramId(new ProgramId(15), 3), Is.EqualTo(new ProgramId(20)));
        Assert.That(manager.GetProgramId(new ProgramId(20), 3), Is.EqualTo(new ProgramId(20)));
        Assert.That(manager.GetProgramId(new ProgramId(25), 3), Is.EqualTo(new ProgramId(20)));

        // Check that any index can be used
        Assert.That(manager.GetProgramId(new ProgramId(10), 0), Is.EqualTo(new ProgramId(5)));
        Assert.That(manager.GetProgramId(new ProgramId(10), 1), Is.EqualTo(new ProgramId(10)));
        Assert.That(manager.GetProgramId(new ProgramId(10), 2), Is.EqualTo(new ProgramId(15)));
        Assert.That(manager.GetProgramId(new ProgramId(10), 3), Is.EqualTo(new ProgramId(20)));
        Assert.That(manager.GetProgramId(new ProgramId(10), 4), Is.EqualTo(new ProgramId(25)));
    }

    [Test]
    public void GetProgramId_InputIdDoesNotExist_ReturnsInvalidId()
    {
        const int count = 5;

        ProgramIndexMapInfoManager manager = CreatePopulatedManager(count, x => x + 1);

        Assert.That(manager.GetProgramId(new ProgramId(0), 3), Is.EqualTo(ProgramId.InvalidId));
        Assert.That(manager.GetProgramId(new ProgramId(666), 3), Is.EqualTo(ProgramId.InvalidId));
        Assert.That(manager.GetProgramId(new ProgramId(777), 3), Is.EqualTo(ProgramId.InvalidId));
    }

    [Test]
    public void GetProgramId_InputIndexDoesNotExist_ReturnsInvalidId()
    {
        const int count = 5;

        ProgramIndexMapInfoManager manager = CreatePopulatedManager(count, x => x + 1);

        Assert.That(manager.GetProgramId(new ProgramId(2), 5), Is.EqualTo(ProgramId.InvalidId));
        Assert.That(manager.GetProgramId(new ProgramId(2), 12), Is.EqualTo(ProgramId.InvalidId));
        Assert.That(manager.GetProgramId(new ProgramId(2), 255), Is.EqualTo(ProgramId.InvalidId));
    }

    [Test]
    public void GetProgramCount_MapIsEmpty_ReturnsZero()
    {
        var manager = new ProgramIndexMapInfoManager();

        Assert.That(manager.GetProgramCount(), Is.EqualTo(0));
    }

    [Test]
    public void GetProgramCount_MapHasEntries_ReturnsCorrectCount()
    {
        ProgramIndexMapInfoManager manager = CreatePopulatedManager(1, x => x + 1);
        Assert.That(manager.GetProgramCount(), Is.EqualTo(1));

        manager = CreatePopulatedManager(10, x => x + 1);
        Assert.That(manager.GetProgramCount(), Is.EqualTo(10));

        manager = CreatePopulatedManager(255, x => x + 1);
        Assert.That(manager.GetProgramCount(), Is.EqualTo(255));
    }

    [Test]
    public void Clear_MapHasEntries_CountIsZero()
    {
        const int count = 5;
        ProgramIndexMapInfoManager manager = CreatePopulatedManager(count, x => x + 1);

        manager.Clear();

        Assert.That(manager.GetProgramCount(), Is.EqualTo(0));
    }

    [Test]
    public void Clear_MapHasEntries_CannotGetOldEntry()
    {
        const int count = 5;
        ProgramIndexMapInfoManager manager = CreatePopulatedManager(count, x => x + 1);

        manager.Clear();

        Assert.That(manager.Get(new ProgramId(2)).HasValue, Is.False);
    }
}
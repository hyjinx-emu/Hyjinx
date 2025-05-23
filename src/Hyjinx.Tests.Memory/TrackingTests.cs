using Hyjinx.Memory;
using Hyjinx.Memory.Tracking;
using System.Diagnostics;

namespace Hyjinx.Tests.Memory;

public class TrackingTests
{
    private const int RndCnt = 3;

    private const ulong MemorySize = 0x8000;
    private const int PageSize = 4096;

    private MemoryBlock _memoryBlock;
    private MemoryTracking _tracking;
    private MockVirtualMemoryManager _memoryManager;

    [SetUp]
    public void Setup()
    {
        _memoryBlock = new MemoryBlock(MemorySize);
        _memoryManager = new MockVirtualMemoryManager(MemorySize, PageSize);
        _tracking = new MemoryTracking(_memoryManager, PageSize);
    }

    [TearDown]
    public void Teardown()
    {
        _memoryBlock.Dispose();
    }

    private bool TestSingleWrite(RegionHandle handle, ulong address, ulong size)
    {
        handle.Reprotect();

        _tracking.VirtualMemoryEvent(address, size, true);

        return handle.Dirty;
    }

    [Test]
    public void SingleRegion()
    {
        RegionHandle handle = _tracking.BeginTracking(0, PageSize, 0);
        (ulong address, ulong size)? readTrackingTriggered = null;
        handle.RegisterAction((address, size) =>
        {
            readTrackingTriggered = (address, size);
        });

        bool dirtyInitial = handle.Dirty;
        Assert.That(dirtyInitial); // Handle starts dirty.

        handle.Reprotect();

        bool dirtyAfterReprotect = handle.Dirty;
        Assert.That(!dirtyAfterReprotect); // Handle is no longer dirty.

        _tracking.VirtualMemoryEvent(PageSize * 2, 4, true);
        _tracking.VirtualMemoryEvent(PageSize * 2, 4, false);

        bool dirtyAfterUnrelatedReadWrite = handle.Dirty;
        Assert.That(!dirtyAfterUnrelatedReadWrite); // Not dirtied, as the write was to an unrelated address.

        ClassicAssert.IsNull(readTrackingTriggered); // Hasn't been triggered yet

        _tracking.VirtualMemoryEvent(0, 4, false);

        bool dirtyAfterRelatedRead = handle.Dirty;
        Assert.That(!dirtyAfterRelatedRead); // Only triggers on write.
        ClassicAssert.AreEqual(readTrackingTriggered, (0UL, 4UL)); // Read action was triggered.

        readTrackingTriggered = null;
        _tracking.VirtualMemoryEvent(0, 4, true);

        bool dirtyAfterRelatedWrite = handle.Dirty;
        Assert.That(dirtyAfterRelatedWrite); // Dirty flag should now be set.

        _tracking.VirtualMemoryEvent(4, 4, true);
        bool dirtyAfterRelatedWrite2 = handle.Dirty;
        Assert.That(dirtyAfterRelatedWrite2); // Dirty flag should still be set.

        handle.Reprotect();

        bool dirtyAfterReprotect2 = handle.Dirty;
        Assert.That(!dirtyAfterReprotect2); // Handle is no longer dirty.

        handle.Dispose();

        bool dirtyAfterDispose = TestSingleWrite(handle, 0, 4);
        Assert.That(!dirtyAfterDispose); // Handle cannot be triggered when disposed
    }

    [Test]
    public void OverlappingRegions()
    {
        RegionHandle allHandle = _tracking.BeginTracking(0, PageSize * 16, 0);
        allHandle.Reprotect();

        (ulong address, ulong size)? readTrackingTriggeredAll = null;

        void RegisterReadAction()
        {
            readTrackingTriggeredAll = null;
            allHandle.RegisterAction((address, size) =>
            {
                readTrackingTriggeredAll = (address, size);
            });
        }

        RegisterReadAction();

        // Create 16 page sized handles contained within the allHandle.
        RegionHandle[] containedHandles = new RegionHandle[16];

        for (int i = 0; i < 16; i++)
        {
            containedHandles[i] = _tracking.BeginTracking((ulong)i * PageSize, PageSize, 0);
            containedHandles[i].Reprotect();
        }

        for (int i = 0; i < 16; i++)
        {
            // No handles are dirty.
            Assert.That(!allHandle.Dirty);
            ClassicAssert.IsNull(readTrackingTriggeredAll);
            for (int j = 0; j < 16; j++)
            {
                Assert.That(!containedHandles[j].Dirty);
            }

            _tracking.VirtualMemoryEvent((ulong)i * PageSize, 1, true);

            // Only the handle covering the entire range and the relevant contained handle are dirty.
            Assert.That(allHandle.Dirty);
            ClassicAssert.AreEqual(readTrackingTriggeredAll, ((ulong)i * PageSize, 1UL)); // Triggered read tracking
            for (int j = 0; j < 16; j++)
            {
                if (j == i)
                {
                    Assert.That(containedHandles[j].Dirty);
                }
                else
                {
                    Assert.That(!containedHandles[j].Dirty);
                }
            }

            // Clear flags and reset read action.
            RegisterReadAction();
            allHandle.Reprotect();
            containedHandles[i].Reprotect();
        }
    }

    [Test]
    public void PageAlignment(
        [Values(1ul, 512ul, 2048ul, 4096ul, 65536ul)][Random(1ul, 65536ul, RndCnt)] ulong address,
        [Values(1ul, 4ul, 1024ul, 4096ul, 65536ul)][Random(1ul, 65536ul, RndCnt)] ulong size)
    {
        ulong alignedStart = (address / PageSize) * PageSize;
        ulong alignedEnd = ((address + size + PageSize - 1) / PageSize) * PageSize;
        ulong alignedSize = alignedEnd - alignedStart;

        RegionHandle handle = _tracking.BeginTracking(address, size, 0);

        // Anywhere inside the pages the region is contained on should trigger.

        bool originalRangeTriggers = TestSingleWrite(handle, address, size);
        Assert.That(originalRangeTriggers);

        bool alignedRangeTriggers = TestSingleWrite(handle, alignedStart, alignedSize);
        Assert.That(alignedRangeTriggers);

        bool alignedStartTriggers = TestSingleWrite(handle, alignedStart, 1);
        Assert.That(alignedStartTriggers);

        bool alignedEndTriggers = TestSingleWrite(handle, alignedEnd - 1, 1);
        Assert.That(alignedEndTriggers);

        // Outside the tracked range should not trigger.

        bool alignedBeforeTriggers = TestSingleWrite(handle, alignedStart - 1, 1);
        Assert.That(!alignedBeforeTriggers);

        bool alignedAfterTriggers = TestSingleWrite(handle, alignedEnd, 1);
        Assert.That(!alignedAfterTriggers);
    }

    [Test, Explicit, Timeout(1000)]
    public void Multithreading()
    {
        // Multithreading sanity test
        // Multiple threads can easily read/write memory regions from any existing handle.
        // Handles can also be owned by different threads, though they should have one owner thread.
        // Handles can be created and disposed at any time, by any thread.

        // This test should not throw or deadlock due to invalid state.

        const int ThreadCount = 1;
        const int HandlesPerThread = 16;
        long finishedTime = 0;

        RegionHandle[] handles = new RegionHandle[ThreadCount * HandlesPerThread];
        Random globalRand = new();

        for (int i = 0; i < handles.Length; i++)
        {
            handles[i] = _tracking.BeginTracking((ulong)i * PageSize, PageSize, 0);
            handles[i].Reprotect();
        }

        List<Thread> testThreads = new();

        // Dirty flag consumer threads
        int dirtyFlagReprotects = 0;
        for (int i = 0; i < ThreadCount; i++)
        {
            int randSeed = i;
            testThreads.Add(new Thread(() =>
            {
                int handleBase = randSeed * HandlesPerThread;
                while (Stopwatch.GetTimestamp() < finishedTime)
                {
                    Random random = new(randSeed);
                    RegionHandle handle = handles[handleBase + random.Next(HandlesPerThread)];

                    if (handle.Dirty)
                    {
                        handle.Reprotect();
                        Interlocked.Increment(ref dirtyFlagReprotects);
                    }
                }
            }));
        }

        // Write trigger threads
        int writeTriggers = 0;
        for (int i = 0; i < ThreadCount; i++)
        {
            int randSeed = i;
            testThreads.Add(new Thread(() =>
            {
                Random random = new(randSeed);
                ulong handleBase = (ulong)(randSeed * HandlesPerThread * PageSize);
                while (Stopwatch.GetTimestamp() < finishedTime)
                {
                    _tracking.VirtualMemoryEvent(handleBase + (ulong)random.Next(PageSize * HandlesPerThread), PageSize / 2, true);
                    Interlocked.Increment(ref writeTriggers);
                }
            }));
        }

        // Handle create/delete threads
        int handleLifecycles = 0;
        for (int i = 0; i < ThreadCount; i++)
        {
            int randSeed = i;
            testThreads.Add(new Thread(() =>
            {
                int maxAddress = ThreadCount * HandlesPerThread * PageSize;
                Random random = new(randSeed + 512);
                while (Stopwatch.GetTimestamp() < finishedTime)
                {
                    RegionHandle handle = _tracking.BeginTracking((ulong)random.Next(maxAddress), (ulong)random.Next(65536), 0);

                    handle.Dispose();

                    Interlocked.Increment(ref handleLifecycles);
                }
            }));
        }

        finishedTime = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2; // Run for 500ms;

        foreach (Thread thread in testThreads)
        {
            thread.Start();
        }

        foreach (Thread thread in testThreads)
        {
            thread.Join();
        }

        ClassicAssert.Greater(dirtyFlagReprotects, 10);
        ClassicAssert.Greater(writeTriggers, 10);
        ClassicAssert.Greater(handleLifecycles, 10);
    }

    [Test]
    public void ReadActionThreadConsumption()
    {
        // Read actions should only be triggered once for each registration.
        // The implementation should use an interlocked exchange to make sure other threads can't get the action.

        RegionHandle handle = _tracking.BeginTracking(0, PageSize, 0);

        int triggeredCount = 0;
        int registeredCount = 0;
        int signalThreadsDone = 0;
        bool isRegistered = false;

        void RegisterReadAction()
        {
            registeredCount++;
            handle.RegisterAction((address, size) =>
            {
                isRegistered = false;
                Interlocked.Increment(ref triggeredCount);
            });
        }

        const int ThreadCount = 16;
        const int IterationCount = 10000;
        Thread[] signalThreads = new Thread[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            int randSeed = i;
            signalThreads[i] = new Thread(() =>
            {
                Random random = new(randSeed);
                for (int j = 0; j < IterationCount; j++)
                {
                    _tracking.VirtualMemoryEvent((ulong)random.Next(PageSize), 4, false);
                }
                Interlocked.Increment(ref signalThreadsDone);
            });
        }

        for (int i = 0; i < ThreadCount; i++)
        {
            signalThreads[i].Start();
        }

        while (signalThreadsDone != -1)
        {
            if (signalThreadsDone == ThreadCount)
            {
                signalThreadsDone = -1;
            }

            if (!isRegistered)
            {
                isRegistered = true;
                RegisterReadAction();
            }
        }

        // The action should trigger exactly once for every registration,
        // then we register once after all the threads signalling it cease.
        ClassicAssert.AreEqual(registeredCount, triggeredCount + 1);
    }

    [Test]
    public void DisposeHandles()
    {
        // Ensure that disposed handles correctly remove their virtual and physical regions.

        RegionHandle handle = _tracking.BeginTracking(0, PageSize, 0);
        handle.Reprotect();

        ClassicAssert.AreEqual(1, _tracking.GetRegionCount());

        handle.Dispose();

        ClassicAssert.AreEqual(0, _tracking.GetRegionCount());

        // Two handles, small entirely contains big.
        // We expect there to be three regions after creating both, one for the small region and two covering the big one around it.
        // Regions are always split to avoid overlapping, which is why there are three instead of two.

        RegionHandle handleSmall = _tracking.BeginTracking(PageSize, PageSize, 0);
        RegionHandle handleBig = _tracking.BeginTracking(0, PageSize * 4, 0);

        ClassicAssert.AreEqual(3, _tracking.GetRegionCount());

        // After disposing the big region, only the small one will remain.
        handleBig.Dispose();

        ClassicAssert.AreEqual(1, _tracking.GetRegionCount());

        handleSmall.Dispose();

        ClassicAssert.AreEqual(0, _tracking.GetRegionCount());
    }

    [Test]
    public void ReadAndWriteProtection()
    {
        MemoryPermission protection = MemoryPermission.ReadAndWrite;

        _memoryManager.OnProtect += (va, size, newProtection) =>
        {
            ClassicAssert.AreEqual((0, PageSize), (va, size)); // Should protect the exact region all the operations use.
            protection = newProtection;
        };

        RegionHandle handle = _tracking.BeginTracking(0, PageSize, 0);

        // After creating the handle, there is no protection yet.
        ClassicAssert.AreEqual(MemoryPermission.ReadAndWrite, protection);

        bool dirtyInitial = handle.Dirty;
        Assert.That(dirtyInitial); // Handle starts dirty.

        handle.Reprotect();

        // After a reprotect, there is write protection, which will set a dirty flag when any write happens.
        ClassicAssert.AreEqual(MemoryPermission.Read, protection);

        (ulong address, ulong size)? readTrackingTriggered = null;
        handle.RegisterAction((address, size) =>
        {
            readTrackingTriggered = (address, size);
        });

        // Registering an action adds read/write protection.
        ClassicAssert.AreEqual(MemoryPermission.None, protection);

        bool dirtyAfterReprotect = handle.Dirty;
        Assert.That(!dirtyAfterReprotect); // Handle is no longer dirty.

        // First we should read, which will trigger the action. This _should not_ remove write protection on the memory.

        _tracking.VirtualMemoryEvent(0, 4, false);

        bool dirtyAfterRead = handle.Dirty;
        Assert.That(!dirtyAfterRead); // Not dirtied, as this was a read.

        ClassicAssert.AreEqual(readTrackingTriggered, (0UL, 4UL)); // Read action was triggered.

        ClassicAssert.AreEqual(MemoryPermission.Read, protection); // Write protection is still present.

        readTrackingTriggered = null;

        // Now, perform a write.

        _tracking.VirtualMemoryEvent(0, 4, true);

        bool dirtyAfterWriteAfterRead = handle.Dirty;
        Assert.That(dirtyAfterWriteAfterRead); // Should be dirty.

        ClassicAssert.AreEqual(MemoryPermission.ReadAndWrite, protection); // All protection is now be removed from the memory.

        ClassicAssert.IsNull(readTrackingTriggered); // Read tracking was removed when the action fired, as it can only fire once.

        handle.Dispose();
    }

    [Test]
    public void PreciseAction()
    {
        RegionHandle handle = _tracking.BeginTracking(0, PageSize, 0);

        (ulong address, ulong size, bool write)? preciseTriggered = null;
        handle.RegisterPreciseAction((address, size, write) =>
        {
            preciseTriggered = (address, size, write);

            return true;
        });

        (ulong address, ulong size)? readTrackingTriggered = null;
        handle.RegisterAction((address, size) =>
        {
            readTrackingTriggered = (address, size);
        });

        handle.Reprotect();

        _tracking.VirtualMemoryEvent(0, 4, false, precise: true);

        ClassicAssert.IsNull(readTrackingTriggered); // Hasn't been triggered - precise action returned true.
        ClassicAssert.AreEqual(preciseTriggered, (0UL, 4UL, false)); // Precise action was triggered.

        _tracking.VirtualMemoryEvent(0, 4, true, precise: true);

        ClassicAssert.IsNull(readTrackingTriggered); // Still hasn't been triggered.
        bool dirtyAfterPreciseActionTrue = handle.Dirty;
        Assert.That(!dirtyAfterPreciseActionTrue); // Not dirtied - precise action returned true.
        ClassicAssert.AreEqual(preciseTriggered, (0UL, 4UL, true)); // Precise action was triggered.

        // Handle is now dirty.
        handle.Reprotect(true);
        preciseTriggered = null;

        _tracking.VirtualMemoryEvent(4, 4, true, precise: true);
        ClassicAssert.AreEqual(preciseTriggered, (4UL, 4UL, true)); // Precise action was triggered even though handle was dirty.

        handle.Reprotect();
        handle.RegisterPreciseAction((address, size, write) =>
        {
            preciseTriggered = (address, size, write);

            return false; // Now, we return false, which indicates that the regular read/write behaviours should trigger.
        });

        _tracking.VirtualMemoryEvent(8, 4, true, precise: true);

        ClassicAssert.AreEqual(readTrackingTriggered, (8UL, 4UL)); // Read action triggered, as precise action returned false.
        bool dirtyAfterPreciseActionFalse = handle.Dirty;
        Assert.That(dirtyAfterPreciseActionFalse); // Dirtied, as precise action returned false.
        ClassicAssert.AreEqual(preciseTriggered, (8UL, 4UL, true)); // Precise action was triggered.
    }
}
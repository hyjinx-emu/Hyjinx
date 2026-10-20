using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Kvdb;
using LibHac.Tests.Fs.FileSystemClientTests;
using LibHac.Tools.Fs;
using System;
using NUnit.Framework;
using TTest = System.Int32;

namespace LibHac.Tests.Kvdb;

public class FlatMapKeyValueStoreTests
{
    private static void AssertSuccess(Result result)
    {
        Assert.That(result.IsSuccess(), Is.True, result.ToString());
    }

    private static void AssertResult(Result.Base expected, Result actual)
    {
        Assert.That(expected.Includes(actual), Is.True, actual.ToString());
    }

    private static ReadOnlySpan<byte> MountName => "mount"u8;
    private static ReadOnlySpan<byte> RootPath => "mount:/"u8;
    private static ReadOnlySpan<byte> ArchiveFilePath => "mount:/imkvdb.arc"u8;

    private static (FlatMapKeyValueStore<T> kvStore, FileSystemClient fsClient) Create<T>(int capacity)
        where T : unmanaged, IEquatable<T>, IComparable<T>
    {
        FileSystemClient fsClient = FileSystemServerFactory.CreateClient(false);

        using var mountedFs = new UniqueRef<IFileSystem>(new InMemoryFileSystem());
        fsClient.Register(MountName, ref mountedFs.Ref).ThrowIfFailure();

        FlatMapKeyValueStore<T> kvStore = Create<T>(fsClient, capacity);

        return (kvStore, fsClient);
    }

    private static FlatMapKeyValueStore<T> Create<T>(FileSystemClient fsClient, int capacity)
        where T : unmanaged, IEquatable<T>, IComparable<T>
    {
        var memoryResource = new ArrayPoolMemoryResource();

        var kvStore = new FlatMapKeyValueStore<T>();
        kvStore.Initialize(fsClient, RootPath, capacity, memoryResource, memoryResource).ThrowIfFailure();

        return kvStore;
    }

    private static byte[][] GenerateValues(int count, int startingSize)
    {
        byte[][] values = new byte[count][];

        for (int i = 0; i < count; i++)
        {
            byte[] value = new byte[startingSize + i];
            value.AsSpan().Fill(unchecked((byte)count));
            values[i] = value;
        }

        return values;
    }

    private static Result PopulateKvStore(FlatMapKeyValueStore<TTest> kvStore, out byte[][] addedValues, int count,
        int startingValueSize = 20, int seed = -1)
    {
        addedValues = null;
        byte[][] values = GenerateValues(count, startingValueSize);

        if (seed == -1)
        {
            for (TTest i = 0; i < count; i++)
            {
                Result res = kvStore.Set(in i, values[i]);
                if (res.IsFailure())
                    return res.Miss();
            }
        }
        else
        {
            var rng = new FullCycleRandom(count, seed);

            for (int i = 0; i < count; i++)
            {
                TTest index = rng.Next();
                Result res = kvStore.Set(in index, values[index]);
                if (res.IsFailure())
                    return res.Miss();
            }
        }

        addedValues = values;
        return Result.Success;
    }

    [Test]
    public void Count_EmptyStore_ReturnsZero()
    {
        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(10);

        Assert.That(kvStore.Count, Is.EqualTo(0));
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    public void Count_PopulatedStore_ReturnsCorrectCount(int count)
    {
        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(10);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        Assert.That(kvStore.Count, Is.EqualTo(count));
    }

    [Test]
    public void Load_FileDoesNotExist_ExistingEntriesAreCleared()
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        AssertSuccess(kvStore.Load());
        Assert.That(kvStore.Count, Is.EqualTo(0));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public void Load_AfterArchiveHasBeenSaved_AllEntriesAreLoaded(int count)
    {
        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient fsClient) = Create<TTest>(count + 5);
        AssertSuccess(PopulateKvStore(kvStore, out byte[][] values, count));

        AssertSuccess(kvStore.Save());
        kvStore.Dispose();

        kvStore = Create<TTest>(fsClient, count + 5);
        AssertSuccess(kvStore.Load());

        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetBeginIterator();

        // Check if each key-value pair matches
        for (int i = 0; i < count; i++)
        {
            TTest expectedKey = i;
            byte[] expectedValue = values[i];

            ref FlatMapKeyValueStore<TTest>.KeyValue kv = ref iterator.Get();

            Assert.That(kv.Key, Is.EqualTo(expectedKey));
            Assert.That(kv.Value.Span.ToArray(), Is.EqualTo(expectedValue));

            iterator.Next();
        }

        Assert.That(iterator.IsEnd(), Is.True);
    }

    [Test]
    public void Load_CapacityIsTooSmall_ReturnsOutOfKeyResource()
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient fsClient) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        AssertSuccess(kvStore.Save());
        kvStore.Dispose();


        kvStore = Create<TTest>(fsClient, count - 5);
        AssertResult(ResultKvdb.OutOfKeyResource, kvStore.Load());
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public void Save_ArchiveFileIsWrittenToDisk(int count)
    {
        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient fsClient) = Create<TTest>(count + 5);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        AssertSuccess(kvStore.Save());

        AssertSuccess(fsClient.GetEntryType(out DirectoryEntryType entryType, ArchiveFilePath));
        Assert.That(entryType, Is.EqualTo(DirectoryEntryType.File));
    }

    [Test]
    public void Get_PopulatedStoreAndEntryDoesNotExist_ReturnsKeyNotFound()
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        TTest key = 20;
        byte[] value = new byte[20];

        Result res = kvStore.Get(out int _, in key, value);
        AssertResult(ResultKvdb.KeyNotFound, res);
    }

    [Test]
    public void Get_PopulatedStore_GetsCorrectValueSizes()
    {
        const int count = 10;
        const int startingValueSize = 20;
        const int rngSeed = 220;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count, startingValueSize, rngSeed));

        // Check the size of each entry
        byte[] value = new byte[100];

        for (TTest i = 0; i < count; i++)
        {
            AssertSuccess(kvStore.Get(out int valueSize, in i, value));
            Assert.That(valueSize, Is.EqualTo(startingValueSize + i));
        }
    }

    [Test]
    public void Get_PopulatedStoreAndEntryExists_GetsCorrectValue()
    {
        const int count = 10;
        const int startingValueSize = 20;
        const int rngSeed = 188;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out byte[][] values, count, startingValueSize, rngSeed));

        // Check if each value matches
        byte[] value = new byte[100];

        for (int i = 0; i < count; i++)
        {
            TTest key = i;
            AssertSuccess(kvStore.Get(out int _, in key, value));
            Assert.That(value.AsSpan(0, startingValueSize + i).ToArray(), Is.EqualTo(values[i]));
        }
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(10)]
    public void Set_StoreIsFullAndEntryDoesNotExist_ReturnsOutOfKeyResource(int count)
    {
        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out byte[][] values, count));

        TTest key = count;
        Result res = kvStore.Set(in key, values[0]);

        AssertResult(ResultKvdb.OutOfKeyResource, res);
    }

    [TestCase(0)]
    [TestCase(5)]
    [TestCase(9)]
    public void Set_StoreIsFullAndEntryAlreadyExists_ReplacesOriginalValue(int entryToReplace)
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        TTest key = entryToReplace;
        byte[] value = new byte[15];
        value.AsSpan().Fill(0xFF);

        AssertSuccess(kvStore.Set(in key, value));

        // Read back the value
        byte[] readValue = new byte[20];
        AssertSuccess(kvStore.Get(out int valueSize, in key, readValue));

        // Check the value contents and size
        Assert.That(valueSize, Is.EqualTo(value.Length));
        Assert.That(readValue.AsSpan(0, valueSize).ToArray(), Is.EqualTo(value));
    }

    [TestCase(10, 89)]
    [TestCase(10, 50)]
    [TestCase(1000, 75367)]
    [TestCase(1000, 117331)]
    public void Set_StoreIsFilledInRandomOrder_EntriesAreSorted(int entryCount, int rngSeed)
    {
        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(entryCount + 10);
        AssertSuccess(PopulateKvStore(kvStore, out byte[][] values, entryCount, 20, rngSeed));

        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetBeginIterator();

        // Check if each key-value pair matches
        for (int i = 0; i < entryCount; i++)
        {
            TTest expectedKey = i;
            byte[] expectedValue = values[i];

            ref FlatMapKeyValueStore<TTest>.KeyValue kv = ref iterator.Get();

            Assert.That(kv.Key, Is.EqualTo(expectedKey));
            Assert.That(kv.Value.Span.ToArray(), Is.EqualTo(expectedValue));

            iterator.Next();
        }

        Assert.That(iterator.IsEnd(), Is.True);
    }

    [Test]
    public void Delete_EmptyStore_ReturnsKeyNotFound()
    {
        const int count = 10;
        TTest keyToDelete = 4;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);

        Result res = kvStore.Delete(in keyToDelete);
        AssertResult(ResultKvdb.KeyNotFound, res);
    }

    [Test]
    public void Delete_PopulatedStoreAndEntryDoesNotExist_ReturnsKeyNotFound()
    {
        const int count = 10;
        TTest keyToDelete = 44;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        Result res = kvStore.Delete(in keyToDelete);
        AssertResult(ResultKvdb.KeyNotFound, res);
    }

    [TestCase(0)]
    [TestCase(5)]
    [TestCase(9)]
    public void Delete_PopulatedStoreAndEntryExists_CannotGetAfterDeletion(int entryToDelete)
    {
        const int count = 10;
        const int startingValueSize = 20;
        const int rngSeed = 114;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count, startingValueSize, rngSeed));

        TTest keyToDelete = entryToDelete;
        AssertSuccess(kvStore.Delete(in keyToDelete));

        byte[] value = new byte[20];

        Result res = kvStore.Get(out int _, in keyToDelete, value);
        AssertResult(ResultKvdb.KeyNotFound, res);
    }

    [TestCase(0)]
    [TestCase(5)]
    [TestCase(9)]
    public void Delete_PopulatedStoreAndEntryExists_CountIsDecremented(int entryToDelete)
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        TTest keyToDelete = entryToDelete;
        AssertSuccess(kvStore.Delete(in keyToDelete));

        Assert.That(kvStore.Count, Is.EqualTo(count - 1));
    }

    [TestCase(0)]
    [TestCase(5)]
    [TestCase(9)]
    public void Delete_PopulatedStoreAndEntryExists_RemainingEntriesAreSorted(int entryToDelete)
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        TTest keyToDelete = entryToDelete;
        AssertSuccess(kvStore.Delete(in keyToDelete));

        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetBeginIterator();

        // Check if the remaining keys exist in order
        for (int i = 0; i < count; i++)
        {
            if (i == entryToDelete)
                continue;

            TTest expectedKey = i;

            Assert.That(iterator.Get().Key, Is.EqualTo(expectedKey));

            iterator.Next();
        }

        Assert.That(iterator.IsEnd(), Is.True);
    }

    [TestCase(0)]
    [TestCase(5)]
    [TestCase(9)]
    public void GetLowerBoundIterator_EntryExists_StartsIterationAtSpecifiedKey(int startEntry)
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        TTest startingKey = startEntry;
        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetLowerBoundIterator(in startingKey);

        Assert.That(iterator.IsEnd(), Is.False);
        Assert.That(iterator.Get().Key, Is.EqualTo(startingKey));
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(9)]
    public void GetLowerBoundIterator_EntryDoesNotExist_StartsIterationAtNextLargestKey(int startIndex)
    {
        const int count = 10;
        const int startingValueSize = 20;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);

        byte[][] values = GenerateValues(count, startingValueSize);

        for (int i = 0; i < count; i++)
        {
            TTest key = i * 2;
            AssertSuccess(kvStore.Set(in key, values[i]));
        }

        TTest startingKey = startIndex;
        TTest nextLargestKey = startIndex + 1;
        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetLowerBoundIterator(in startingKey);

        Assert.That(iterator.IsEnd(), Is.False);
        Assert.That(iterator.Get().Key, Is.EqualTo(nextLargestKey));
    }

    [Test]
    public void GetLowerBoundIterator_LargerThanAllKeysInStore_IteratorIsAtEnd()
    {
        const int count = 10;
        const int startIndex = 20;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        TTest key = startIndex;
        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetLowerBoundIterator(in key);

        Assert.That(iterator.IsEnd(), Is.True);
    }

    [TestCase(2, 3, 2)]
    [TestCase(3, 3, 4)]
    [TestCase(5, 3, 5)]
    public void FixIterator_RemoveEntry_IteratorPointsToSameEntry(int positionWhenRemoving, int entryToRemove, int expectedNewPosition)
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetBeginIterator();

        while (iterator.Get().Key != positionWhenRemoving)
        {
            iterator.Next();
        }

        TTest keyToRemove = entryToRemove;
        AssertSuccess(kvStore.Delete(in keyToRemove));

        kvStore.FixIterator(ref iterator, in keyToRemove);

        TTest expectedKey = expectedNewPosition;
        Assert.That(iterator.Get().Key, Is.EqualTo(expectedKey));
    }

    [TestCase(6, 7, 6)]
    [TestCase(8, 7, 8)]
    public void FixIterator_AddEntry_IteratorPointsToSameEntry(int positionWhenAdding, int entryToAdd, int expectedNewPosition)
    {
        const int count = 10;
        const int startingValueSize = 20;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count + 5);

        byte[][] values = GenerateValues(count, startingValueSize);

        for (int i = 0; i < count; i++)
        {
            TTest key = i * 2;
            AssertSuccess(kvStore.Set(in key, values[i]));
        }

        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetBeginIterator();

        while (iterator.Get().Key != positionWhenAdding)
        {
            iterator.Next();
        }

        TTest keyToAdd = entryToAdd;
        byte[] valueToAdd = new byte[10];

        AssertSuccess(kvStore.Set(in keyToAdd, valueToAdd));

        kvStore.FixIterator(ref iterator, in keyToAdd);

        TTest expectedKey = expectedNewPosition;
        Assert.That(iterator.Get().Key, Is.EqualTo(expectedKey));
    }

    [Test]
    public void IteratorIsEnd_EmptyStore_ReturnsTrue()
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);

        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetBeginIterator();
        Assert.That(iterator.IsEnd(), Is.True);
    }

    [Test]
    public void IteratorIsEnd_PopulatedStore_ReturnsFalseUntilFinishedIterating()
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out _, count));

        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetBeginIterator();

        for (int i = 0; i < count; i++)
        {
            Assert.That(iterator.IsEnd(), Is.False);
            iterator.Next();
        }

        // Iterated all entries. Should return true now
        Assert.That(iterator.IsEnd(), Is.True);
    }

    [Test]
    public void IteratorGet_PopulatedStore_ReturnsEntriesInOrder()
    {
        const int count = 10;

        (FlatMapKeyValueStore<TTest> kvStore, FileSystemClient _) = Create<TTest>(count);
        AssertSuccess(PopulateKvStore(kvStore, out byte[][] values, count));

        FlatMapKeyValueStore<TTest>.Iterator iterator = kvStore.GetBeginIterator();

        // Check if each key-value pair matches
        for (int i = 0; i < count; i++)
        {
            TTest expectedKey = i;
            byte[] expectedValue = values[i];

            ref FlatMapKeyValueStore<TTest>.KeyValue kv = ref iterator.Get();

            Assert.That(kv.Key, Is.EqualTo(expectedKey));
            Assert.That(kv.Value.Span.ToArray(), Is.EqualTo(expectedValue));

            iterator.Next();
        }
    }
}
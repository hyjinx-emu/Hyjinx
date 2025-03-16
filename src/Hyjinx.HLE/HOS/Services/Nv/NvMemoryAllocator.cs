using Hyjinx.Common.Collections;
using Hyjinx.Common.Logging;
using Hyjinx.Graphics.Gpu.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Hyjinx.HLE.HOS.Services.Nv
{
    partial class NvMemoryAllocator
    {
        private const ulong AddressSpaceSize = 1UL << 40;

        private const ulong DefaultStart = 1UL << 32;
        private const ulong InvalidAddress = 0;

        private static readonly ILogger<NvMemoryAllocator> _logger =
            Logger.DefaultLoggerFactory.CreateLogger<NvMemoryAllocator>();
        
        private const ulong PageSize = MemoryManager.PageSize;
        private const ulong PageMask = MemoryManager.PageMask;

        public const ulong PteUnmapped = MemoryManager.PteUnmapped;

        // Key   --> Start Address of Region
        // Value --> End Address of Region
        private readonly TreeDictionary<ulong, ulong> _tree = new();

        private readonly Dictionary<ulong, LinkedListNode<ulong>> _dictionary = new();
        private readonly LinkedList<ulong> _list = new();

        public NvMemoryAllocator()
        {
            _tree.Add(PageSize, AddressSpaceSize);
            LinkedListNode<ulong> node = _list.AddFirst(PageSize);
            _dictionary[PageSize] = node;
        }

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Allocating range from 0x{start:X} to 0x{end:X}.")]
        private partial void LogAllocatingMemoryRange(ulong start, ulong end);
        
            // $"Created smaller address range from 0x{referenceAddress:X} to 0x{leftEndAddress:X}.");
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Created smaller address range from 0x{referenceAddress:X} to 0x{leftEndAddress:X}.")]
        private partial void LogAllocatedSmallerAddressRange(ulong referenceAddress, ulong leftEndAddress);
        
        /// <summary>
        /// Marks a range of memory as consumed by removing it from the tree.
        /// This function will split memory regions if there is available space.
        /// </summary>
        /// <param name="va">Virtual address at which to allocate</param>
        /// <param name="size">Size of the allocation in bytes</param>
        /// <param name="referenceAddress">Reference to the address of memory where the allocation can take place</param>
        #region Memory Allocation
        public void AllocateRange(ulong va, ulong size, ulong referenceAddress = InvalidAddress)
        {
            lock (_tree)
            {
                LogAllocatingMemoryRange(va, va + size);
                if (referenceAddress != InvalidAddress)
                {
                    ulong endAddress = va + size;
                    ulong referenceEndAddress = _tree.Get(referenceAddress);
                    if (va >= referenceAddress)
                    {
                        // Need Left Node
                        if (va > referenceAddress)
                        {
                            ulong leftEndAddress = va;

                            // Overwrite existing block with its new smaller range.
                            _tree.Add(referenceAddress, leftEndAddress);
                            LogAllocatedSmallerAddressRange(referenceAddress, leftEndAddress);
                        }
                        else
                        {
                            // We need to get rid of the large chunk.
                            _tree.Remove(referenceAddress);
                        }

                        ulong rightSize = referenceEndAddress - endAddress;
                        // If leftover space, create a right node.
                        if (rightSize > 0)
                        {
                            LogAllocatedSmallerAddressRange(endAddress, referenceEndAddress);
                            
                            _tree.Add(endAddress, referenceEndAddress);

                            LinkedListNode<ulong> node = _list.AddAfter(_dictionary[referenceAddress], endAddress);
                            _dictionary[endAddress] = node;
                        }

                        if (va == referenceAddress)
                        {
                            _list.Remove(_dictionary[referenceAddress]);
                            _dictionary.Remove(referenceAddress);
                        }
                    }
                }
            }
        }

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Deallocating address range from 0x{start:X} to 0x{end:X}.")]
        private partial void LogDeallocatingAddressRange(ulong start, ulong end);

        /// <summary>
        /// Marks a range of memory as free by adding it to the tree.
        /// This function will automatically compact the tree when it determines there are multiple ranges of free memory adjacent to each other.
        /// </summary>
        /// <param name="va">Virtual address at which to deallocate</param>
        /// <param name="size">Size of the allocation in bytes</param>
        public void DeallocateRange(ulong va, ulong size)
        {
            lock (_tree)
            {
                LogDeallocatingAddressRange(va, va + size);

                ulong freeAddressStartPosition = _tree.Floor(va);
                if (freeAddressStartPosition != InvalidAddress)
                {
                    LinkedListNode<ulong> node = _dictionary[freeAddressStartPosition];
                    ulong targetPrevAddress = _dictionary[freeAddressStartPosition].Previous != null ? _dictionary[_dictionary[freeAddressStartPosition].Previous.Value].Value : InvalidAddress;
                    ulong targetNextAddress = _dictionary[freeAddressStartPosition].Next != null ? _dictionary[_dictionary[freeAddressStartPosition].Next.Value].Value : InvalidAddress;
                    ulong expandedStart = va;
                    ulong expandedEnd = va + size;

                    while (targetPrevAddress != InvalidAddress)
                    {
                        ulong prevAddress = targetPrevAddress;
                        ulong prevEndAddress = _tree.Get(targetPrevAddress);
                        if (prevEndAddress >= expandedStart)
                        {
                            expandedStart = targetPrevAddress;
                            LinkedListNode<ulong> prevPtr = _dictionary[prevAddress];
                            if (prevPtr.Previous != null)
                            {
                                targetPrevAddress = prevPtr.Previous.Value;
                            }
                            else
                            {
                                targetPrevAddress = InvalidAddress;
                            }
                            node = node.Previous;
                            _tree.Remove(prevAddress);
                            _list.Remove(_dictionary[prevAddress]);
                            _dictionary.Remove(prevAddress);
                        }
                        else
                        {
                            break;
                        }
                    }

                    while (targetNextAddress != InvalidAddress)
                    {
                        ulong nextAddress = targetNextAddress;
                        ulong nextEndAddress = _tree.Get(targetNextAddress);
                        if (nextAddress <= expandedEnd)
                        {
                            expandedEnd = Math.Max(expandedEnd, nextEndAddress);
                            LinkedListNode<ulong> nextPtr = _dictionary[nextAddress];
                            if (nextPtr.Next != null)
                            {
                                targetNextAddress = nextPtr.Next.Value;
                            }
                            else
                            {
                                targetNextAddress = InvalidAddress;
                            }
                            _tree.Remove(nextAddress);
                            _list.Remove(_dictionary[nextAddress]);
                            _dictionary.Remove(nextAddress);
                        }
                        else
                        {
                            break;
                        }
                    }

                    LogDeallocationCreatedNewFreeRange(expandedStart, expandedEnd);

                    _tree.Add(expandedStart, expandedEnd);
                    LinkedListNode<ulong> nodePtr = _list.AddAfter(node, expandedStart);
                    _dictionary[expandedStart] = nodePtr;
                }
            }
        }

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Deallocation resulted in new free range from 0x{start:X} to 0x{end:X}.")]
        private partial void LogDeallocationCreatedNewFreeRange(ulong start, ulong end);

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Searching for a new free address @ 0x{start:X} of size 0x{size:X}.")]
        private partial void LogSearchingForFreeAddress(ulong start, ulong size);

        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Target address set to start of the last available range: 0x{start:X}.")]
        private partial void LogTargetAddressStartAtLastAvailableRange(ulong start);
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Target address set to floor of 0x{address:X}, resulted in 0x{targetAddress:X}.")]
        private partial void LogAddressSetToFloor(ulong address, ulong targetAddress);
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Target address was invalid, set to ceiling of 0x{address:X}, resulted in 0x{targetAddress:X}.")]
        private partial void LogAddressSetToCeiling(ulong address, ulong targetAddress);
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Found a suitable free address range from 0x{targetAddress:X} to 0x{endAddress:X} for 0x{address:X}.")]
        private partial void LogFoundSuitableFreeAddressRange(ulong targetAddress, ulong endAddress, ulong address);
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Address requirements exceeded the available space in the target range.")]
        private partial void LogAddressRequirementsExceededSpaceAvailable();
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Moved search to successor range starting at 0x{targetAddress:X}.")]
        private partial void LogMovedSearchToSuccessorRange(ulong targetAddress);
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Exiting loop, a full pass has already been completed with no suitable free address range.")]
        private partial void LogNoSuitableFreeAddressRange();
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Reached the end of the available free ranges, restarting loop at 0x{targetAddress:X} for 0x{address:X}.")]
        private partial void LogReachedEndOfFreeRangeRestartingLoop(ulong targetAddress, ulong address);
        
        /// <summary>
        /// Gets the address of an unused (free) region of the specified size.
        /// </summary>
        /// <param name="size">Size of the region in bytes</param>
        /// <param name="freeAddressStartPosition">Position at which memory can be allocated</param>
        /// <param name="alignment">Required alignment of the region address in bytes</param>
        /// <param name="start">Start address of the search on the address space</param>
        /// <returns>GPU virtual address of the allocation, or an all ones mask in case of failure</returns>
        public ulong GetFreeAddress(ulong size, out ulong freeAddressStartPosition, ulong alignment = 1, ulong start = DefaultStart)
        {
            // Note: Address 0 is not considered valid by the driver,
            // when 0 is returned it's considered a mapping error.
            lock (_tree)
            {
                LogSearchingForFreeAddress(start, size);
                
                ulong address = start;

                if (alignment == 0)
                {
                    alignment = 1;
                }

                alignment = (alignment + PageMask) & ~PageMask;
                if (address < AddressSpaceSize)
                {
                    bool reachedEndOfAddresses = false;
                    ulong targetAddress;
                    if (start == DefaultStart)
                    {
                        LogTargetAddressStartAtLastAvailableRange(_list.Last!.Value);
                        targetAddress = _list.Last.Value;
                    }
                    else
                    {
                        targetAddress = _tree.Floor(address);
                        LogAddressSetToFloor(address, targetAddress);
                        
                        if (targetAddress == InvalidAddress)
                        {
                            targetAddress = _tree.Ceiling(address);
                            LogAddressSetToCeiling(address, targetAddress);
                        }
                    }
                    while (address < AddressSpaceSize)
                    {
                        if (targetAddress != InvalidAddress)
                        {
                            if (address >= targetAddress)
                            {
                                if (address + size <= _tree.Get(targetAddress))
                                {
                                    LogFoundSuitableFreeAddressRange(targetAddress, _tree.Get(targetAddress), address);
                                    
                                    freeAddressStartPosition = targetAddress;
                                    return address;
                                }
                                else
                                {
                                    LogAddressRequirementsExceededSpaceAvailable();
                                    
                                    LinkedListNode<ulong> nextPtr = _dictionary[targetAddress];
                                    if (nextPtr.Next != null)
                                    {
                                        targetAddress = nextPtr.Next.Value;
                                        LogMovedSearchToSuccessorRange(targetAddress);
                                    }
                                    else
                                    {
                                        if (reachedEndOfAddresses)
                                        {
                                            LogNoSuitableFreeAddressRange();
                                            break;
                                        }
                                        else
                                        {
                                            reachedEndOfAddresses = true;
                                            address = start;
                                            targetAddress = _tree.Floor(address);
                                            
                                            LogReachedEndOfFreeRangeRestartingLoop(targetAddress, address);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                address += PageSize * (targetAddress / PageSize - (address / PageSize));

                                ulong remainder = address % alignment;

                                if (remainder != 0)
                                {
                                    address = (address - remainder) + alignment;
                                }

                                LogResetAndAlignedToAddress(address);

                                if (address + size > AddressSpaceSize && !reachedEndOfAddresses)
                                {
                                    reachedEndOfAddresses = true;
                                    address = start;
                                    targetAddress = _tree.Floor(address);

                                    LogAddressRequirementsExceededCapacityRestartingLoop(targetAddress, address);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                LogNoSuitableRangeFoundReturningInvalidAddress(InvalidAddress);
                
                freeAddressStartPosition = InvalidAddress;
            }

            return PteUnmapped;
        }
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Reset and aligned address to 0x{address:X}.")]
        private partial void LogResetAndAlignedToAddress(ulong address);
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "Address requirements exceeded the capacity of available address space. Restarting the loop at 0x{targetAddress:X} for 0x{address:X}.")]
        private partial void LogAddressRequirementsExceededCapacityRestartingLoop(ulong targetAddress, ulong address);
        
        [LoggerMessage(LogLevel.Debug,
            EventId = (int)LogClass.ServiceNv, EventName = nameof(LogClass.ServiceNv),
            Message = "No suitable address range found; returning 0x{invalidAddress:X}.")]
        private partial void LogNoSuitableRangeFoundReturningInvalidAddress(ulong invalidAddress);
        
        /// <summary>
        /// Checks if a given memory region is mapped or reserved.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address of the page</param>
        /// <param name="size">Size of the allocation in bytes</param>
        /// <param name="freeAddressStartPosition">Nearest lower address that memory can be allocated</param>
        /// <returns>True if the page is mapped or reserved, false otherwise</returns>
        public bool IsRegionInUse(ulong gpuVa, ulong size, out ulong freeAddressStartPosition)
        {
            lock (_tree)
            {
                ulong floorAddress = _tree.Floor(gpuVa);
                freeAddressStartPosition = floorAddress;
                if (floorAddress != InvalidAddress)
                {
                    return !(gpuVa >= floorAddress && ((gpuVa + size) <= _tree.Get(floorAddress)));
                }
            }
            return true;
        }
        #endregion
    }
}

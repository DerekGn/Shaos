/*
* MIT License
*
* Copyright (c) 2025 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Collections;

namespace Shaos.Sdk.Collections.Generic
{
    /// <summary>
    /// An observable list of <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type of the elements of this <see cref="ObservableList{T}"/></typeparam>
    public class ObservableList<T> : IEnumerable<T>
    {
        private readonly IList<T> _items;

        /// <summary>
        /// Create an instance of an <see cref="ObservableList{T}"/>
        /// </summary>
        public ObservableList()
        {
            _items = [];
        }

        /// <summary>
        /// An event that is raised when the contents of the <see cref="ObservableList{T}"/> changes
        /// </summary>
        public event AsyncEventHandler<ListChangedEventArgs<T>>? ListChanged;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ObservableList{T}"/>
        /// </summary>
        public int Count
        {
            get
            {
                return _items.Count;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="ObservableList{T}"/>
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// Adds an item to the <see cref="ObservableList{T}"/>
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ObservableList{T}"/></param>
        public async Task AddAsync(T item)
        {
            _items.Add(item);

            await OnListChangedAsync(new ListChangedEventArgs<T>(ListChangedAction.Add, [item]));
        }

        /// <summary>
        /// Removes all items from the <see cref="ObservableList{T}"/>
        /// </summary>
        public async Task ClearAsync()
        {
            T[] values = [.. _items];

            _items.Clear();

            await OnListChangedAsync(new ListChangedEventArgs<T>(ListChangedAction.Reset, values));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Insert an instance of <typeparamref name="T"/> into the <see cref="ObservableList{T}"/>
        /// </summary>
        /// <param name="index">The insert index</param>
        /// <param name="item">The item of <typeparamref name="T"/> to insert</param>
        public async Task InsertAsync(int index, T item)
        {
            _items.Insert(index, item);

            await OnListChangedAsync(new ListChangedEventArgs<T>(ListChangedAction.Add, [item]));
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableList{T}"/>
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ObservableList{T}"/></param>
        /// <returns> true if item was successfully removed from the <see cref="ObservableList{T}"/> otherwise, false.
        /// This method also returns false if item is not found in the original <see cref="ObservableList{T}"/>
        /// </returns>
        public async Task<bool> RemoveAsync(T item)
        {
            var removed = _items.Remove(item);

            if (removed)
            {
                await OnListChangedAsync(new ListChangedEventArgs<T>(ListChangedAction.Remove, [item]));
            }

            return removed;
        }

        /// <summary>
        /// Removes the <see cref="ObservableList{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public async Task RemoveAtAsync(int index)
        {
            var item = _items[index];

            _items.RemoveAt(index);

            if (item != null)
            {
                await OnListChangedAsync(new ListChangedEventArgs<T>(ListChangedAction.Remove, [item]));
            }
        }

        /// <summary>
        /// Raise the <see cref="ListChanged"/> event
        /// </summary>
        /// <param name="eventArgs">The <see cref="ListChangedEventArgs{T}"/></param>
        protected virtual async Task OnListChangedAsync(ListChangedEventArgs<T> eventArgs)
        {
            if (ListChanged != null)
            {
                await ListChanged.Invoke(this, eventArgs);
            }
        }
    }
}
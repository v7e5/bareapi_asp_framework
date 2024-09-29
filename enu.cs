using static util.Util;

using System;
using System.Linq;
using System.Collections.Generic;

sealed class NonGen: System.Collections.IEnumerable {
  int[] data = {1,2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,
    19, 20, 21, 22, 23, 24, 25, 26};

  public System.Collections.IEnumerator GetEnumerator() => new Rator(this);

  class Rator: System.Collections.IEnumerator, IDisposable {
    NonGen collection;
    int currentIndex = -1;

    public Rator(NonGen items) => this.collection = items;

    public object Current => collection.data[currentIndex];

    public bool MoveNext() {
      cl($"nongen move {currentIndex}");
      return (currentIndex >= collection.data.Length - 1)
        ? false : ++currentIndex < collection.data.Length;
    }

    public void Reset() => currentIndex = -1;

    void IDisposable.Dispose() {
      cl("nongen dispose");
    }
  }
}

sealed class ConGen: IEnumerable<char> {
  string data = "abcdefghijklmnopqrstuvwxyz";

  public IEnumerator<char> GetEnumerator() => new Rator(this);

  System.Collections.IEnumerator
    System.Collections.IEnumerable.GetEnumerator() =>
      throw new Exception("non gen n/a");

  class Rator: IEnumerator<char> {
    ConGen collection;
    int currentIndex = -1;

    public Rator(ConGen items) => this.collection = items;

    public bool MoveNext() {
      cl($"congen move {currentIndex}");
      return ++currentIndex < collection.data.Length;
    }

    public char Current => collection.data[currentIndex];

    public void Reset() => currentIndex = -1;

    void IDisposable.Dispose() {
      cl("congen dispose");
    }

    object System.Collections.IEnumerator.Current =>
    throw new Exception("non gen n/a");
  }
}

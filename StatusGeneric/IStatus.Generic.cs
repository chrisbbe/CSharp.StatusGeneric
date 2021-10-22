// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace StatusGeneric
{
  /// <summary>
  /// This is a version of <see cref="IStatus"/> that contains a result.
  /// Useful if you want to return something with the status
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IStatus<out T> : IStatus
  {
    /// <summary>
    /// This contains the return result, or if there are errors it will retunr default(T)
    /// </summary>
    T Result { get; }
  }
}
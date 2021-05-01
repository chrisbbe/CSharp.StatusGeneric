// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;

namespace StatusGeneric
{
  /// <summary>
  /// This contains the error handling part of the GenericBizRunner
  /// </summary>
  public class StatusGenericHandler<T> : StatusGenericHandler, IStatusGeneric<T>
  {
    private T _result;

    /// <summary>
    /// This is the returned result
    /// </summary>
    public T Result => IsValid ? _result : default;

    /// <summary>
    /// This allows statuses to be combined. Copies over any errors and replaces the Message if the currect message is null
    /// If you are using Headers then it will combine the headers in any errors in combines
    /// e.g. Status1 with header "MyClass" combines Status2 which has header "MyProp" and status2 has errors.
    /// The result would be error message in status2 would be updates to start with "MyClass>MyProp: This is my error message."
    /// </summary>
    /// <param name="status"></param>
    public IStatusGeneric<T> CombineStatuses(IStatusGeneric<T> status)
    {
      if (!status.IsValid)
      {
        _errors.AddRange(string.IsNullOrEmpty(Header)
          ? status.Errors
          : status.Errors.Select(x => new ErrorGeneric(Header, x)));
      }

      if (IsValid && status.Message != DefaultSuccessMessage)
        Message = status.Message;

      StatusCode ??= status.StatusCode;

      return this;
    }

    /// <summary>
    /// This sets the result to be returned
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public StatusGenericHandler<T> SetResult(T result)
    {
      _result = result;
      return this;
    }

    public StatusGenericHandler<T> SetResult(HttpStatusCode statusCode, T result)
    {
      StatusCode = statusCode;
      _result = result;
      return this;
    }

    /// <summary>
    /// This adds one error to the Errors collection
    /// </summary>
    /// <param name="errorMessage">The text of the error message</param>
    /// <param name="propertyNames">optional. A list of property names that this error applies to</param>
    public new IStatusGeneric<T> AddError(string errorMessage, params string[] propertyNames)
    {
      if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));
      _errors.Add(new ErrorGeneric(Header, new ValidationResult(errorMessage, propertyNames)));
      return this;
    }

    /// <summary>
    /// This adds one error with the associated HTTP Status Code for that error to the Errors collection.
    /// </summary>
    /// <param name="httpStatusCode">The HTTP Status Code for the error.</param>
    /// <param name="errorMessage">The text of the error message.</param>
    /// <param name="propertyNames">Optional: A list of property names that this error applies to.</param>
    public new IStatusGeneric<T> AddError(
      HttpStatusCode httpStatusCode,
      string errorMessage,
      params string[] propertyNames
    )
    {
      if (errorMessage is null) throw new ArgumentNullException(nameof(errorMessage));
      StatusCode = httpStatusCode;
      _errors.Add(new ErrorGeneric(
        Header,
        httpStatusCode,
        new ValidationResult(errorMessage, propertyNames)
      ));
      return this;
    }
  }
}
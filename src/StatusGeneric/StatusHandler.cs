﻿// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;

namespace StatusGeneric
{
  /// <summary>
  /// This contains the error handling part of the GenericBizRunner
  /// </summary>
  public class StatusHandler : IStatusHandler
  {
    /// <summary>
    /// This is the default success message.
    /// </summary>
    public const string DefaultSuccessMessage = "Success";

    protected readonly List<Error> _errors = new();
    private string _successMessage = DefaultSuccessMessage;

    /// <summary>
    /// This creates a StatusHandler, with optional header (see Header property, and CombineStatuses)
    /// </summary>
    /// <param name="header"></param>
    public StatusHandler(string header = "")
    {
      Header = header;
    }

    public HttpStatusCode? StatusCode { get; protected set; }

    /// <summary>
    /// The header provides a prefix to any errors you add. Useful if you want to have a general prefix to all your errors
    /// e.g. a header if "MyClass" would produce error messages such as "MyClass: This is my error message."
    /// </summary>
    public string Header { get; set; }

    /// <summary>
    /// This holds the list of ValidationResult errors. If the collection is empty, then there were no errors
    /// </summary>
    public IReadOnlyList<Error> Errors => _errors.AsReadOnly();

    /// <summary>
    /// This is true if there are no errors 
    /// </summary>
    public bool IsValid => !_errors.Any();

    /// <summary>
    /// This is true if any errors have been added 
    /// </summary>
    public bool HasErrors => _errors.Any();

    /// <summary>
    /// On success this returns the message as set by the business logic, or the default messages set by the BizRunner
    /// If there are errors it contains the message "Failed with NN errors"
    /// </summary>
    public string Message
    {
      get => IsValid
        ? _successMessage
        : $"Failed with {_errors.Count} error" + (_errors.Count == 1 ? "" : "s");
      set => _successMessage = value;
    }

    public StatusHandler SetStatus(HttpStatusCode statusCode)
    {
      StatusCode = statusCode;
      return this;
    }

    /// <summary>
    /// This allows statuses to be combined. Copies over any errors and replaces the Message if the currect message is null
    /// If you are using Headers then it will combine the headers in any errors in combines
    /// e.g. Status1 with header "MyClass" combines Status2 which has header "MyProp" and status2 has errors.
    /// The result would be error message in status2 would be updates to start with "MyClass>MyProp: This is my error message."
    /// </summary>
    /// <param name="status"></param>
    public IStatus CombineStatuses(IStatus status)
    {
      if (!status.IsValid)
      {
        _errors.AddRange(string.IsNullOrEmpty(Header)
          ? status.Errors
          : status.Errors.Select(x => new Error(Header, x)));
      }

      if (IsValid && status.Message != DefaultSuccessMessage)
        Message = status.Message;

      StatusCode ??= status.StatusCode;

      return this;
    }

    /// <summary>
    /// This is a simple method to output all the errors as a single string - null if no errors
    /// Useful for feeding back all the errors in a single exception (also nice in unit testing)
    /// </summary>
    /// <param name="separator">if null then each errors is separated by Environment.NewLine, otherwise uses the separator you provide</param>
    /// <returns>a single string with all errors separated by the 'separator' string</returns>
    public string GetAllErrors(string separator = null)
    {
      separator ??= Environment.NewLine;
      return _errors.Any()
        ? string.Join(separator, Errors)
        : null;
    }

    ///<inheritdoc/>
    public HttpStatusCode? GetLastStatusCode() => HasErrors ? _errors.Last().StatusCode : StatusCode;

    /// <summary>
    /// This adds one error to the Errors collection
    /// NOTE: This is virtual so that the StatusHandler.Generic can override it. That allows both to return a IStatus result
    /// </summary>
    /// <param name="errorMessage">The text of the error message</param>
    /// <param name="propertyNames">optional. A list of property names that this error applies to</param>
    public virtual IStatus AddError(string errorMessage, params string[] propertyNames)
    {
      if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));
      _errors.Add(new Error(Header, new ValidationResult(errorMessage, propertyNames)));
      return this;
    }

    public IStatus AddError(HttpStatusCode statusCode, string errorMessage, params string[] propertyNames)
    {
      if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));
      _errors.Add(new Error(Header, statusCode, new ValidationResult(errorMessage, propertyNames)));
      return this;
    }

    /// <summary>
    /// This adds one error to the Errors collection and saves the exception's data to the DebugData property
    /// </summary>
    /// <param name="ex">The exception that you want to turn into a IStatus error.</param>
    /// <param name="errorMessage">The user-friendly text for the error message</param>
    /// <param name="propertyNames">optional. A list of property names that this error applies to</param>
    public IStatus AddError(Exception ex, string errorMessage, params string[] propertyNames)
    {
      if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));
      var errorGeneric = new Error(Header, new ValidationResult(errorMessage, propertyNames));
      errorGeneric.CopyExceptionToDebugData(ex);
      _errors.Add(errorGeneric);
      return this;
    }

    public IStatus AddError(HttpStatusCode statusCode, Exception ex, string errorMessage,
      params string[] propertyNames)
    {
      if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));
      _errors.Add(new Error(Header, statusCode, new ValidationResult(errorMessage, propertyNames)));
      return this;
    }

    /// <summary>
    /// This adds one ValidationResult to the Errors collection
    /// </summary>
    /// <param name="validationResult"></param>
    public void AddValidationResult(ValidationResult validationResult)
    {
      _errors.Add(new Error(Header, validationResult));
    }

    public void AddValidationResult(HttpStatusCode statusCode, ValidationResult validationResult)
    {
      _errors.Add(new Error(Header, statusCode, validationResult));
    }

    /// <summary>
    /// This appends a collection of ValidationResults to the Errors collection
    /// </summary>
    /// <param name="validationResults"></param>
    public void AddValidationResults(IEnumerable<ValidationResult> validationResults)
    {
      _errors.AddRange(validationResults.Select(x => new Error(Header, x)));
    }

    public void AddValidationResults(HttpStatusCode statusCode, IEnumerable<ValidationResult> validationResults)
    {
      _errors.AddRange(validationResults
        .Select(x => new Error(Header, statusCode, x)));
    }

    public void RunAndCatchEx(
      Action actionToRun,
      HttpStatusCode errorStatusCode = default,
      HttpStatusCode successStatusCode = default
    )
    {
      try
      {
        actionToRun();
      }
      catch (Exception ex)
      {
        if (errorStatusCode is not default(HttpStatusCode))
          AddError(errorStatusCode, ex, ex.Message);
        else
          AddError(ex, ex.Message);
      }

      StatusCode = successStatusCode is default(HttpStatusCode) ? null : successStatusCode;
    }

    public void RunAndCatchEx<TException>(
      Action actionToRun,
      HttpStatusCode errorStatusCode = default,
      HttpStatusCode successStatusCode = default
    ) where TException : Exception
    {
      try
      {
        actionToRun();
      }
      catch (TException ex)
      {
        if (errorStatusCode is not default(HttpStatusCode))
          AddError(errorStatusCode, ex, ex.Message);
        else
          AddError(ex, ex.Message);
      }

      StatusCode = successStatusCode is default(HttpStatusCode) ? null : successStatusCode;
    }

    public T RunAndCatchEx<T>(
      Func<T> funcToRun,
      HttpStatusCode errorStatusCode = default,
      HttpStatusCode successStatusCode = default
    )
    {
      try
      {
        var result = funcToRun();
        StatusCode = successStatusCode is default(HttpStatusCode) ? null : successStatusCode;
        return result;
      }
      catch (Exception ex)
      {
        if (errorStatusCode is not default(HttpStatusCode))
          AddError(errorStatusCode, ex, ex.Message);
        else
          AddError(ex, ex.Message);
      }

      return default;
    }

    public T RunAndCatchEx<T, TException>(
      Func<T> funcToRun,
      HttpStatusCode errorStatusCode = default,
      HttpStatusCode successStatusCode = default
    ) where TException : Exception
    {
      try
      {
        var result = funcToRun();
        StatusCode = successStatusCode is default(HttpStatusCode) ? null : successStatusCode;
        return result;
      }
      catch (TException ex)
      {
        if (errorStatusCode is not default(HttpStatusCode))
          AddError(errorStatusCode, ex, ex.Message);
        else
          AddError(ex, ex.Message);
      }

      return default;
    }
  }
}
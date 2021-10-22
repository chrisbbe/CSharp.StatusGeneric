// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License file in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace StatusGeneric
{
  /// <summary>
  /// This is the interface for the full StatusHandler
  /// </summary>
  public interface IStatusHandler : IStatus
  {
    /// <summary>
    /// This adds one error to the Errors collection
    /// NOTE: This is virtual so that the StatusHandler.Generic can override it. That allows both to return a IStatus result
    /// </summary>
    /// <param name="errorMessage">The text of the error message</param>
    /// <param name="propertyNames">optional. A list of property names that this error applies to</param>
    IStatus AddError(string errorMessage, params string[] propertyNames);

    IStatus AddError(HttpStatusCode statusCode, string errorMessage, params string[] propertyNames);

    /// <summary>
    /// This adds one error to the Errors collection and saves the exception's data to the DebugData property
    /// </summary>
    /// <param name="ex">The exception that you want to turn into a IStatus error.</param>
    /// <param name="errorMessage">The user-friendly text for the error message</param>
    /// <param name="propertyNames">optional. A list of property names that this error applies to</param>
    IStatus AddError(Exception ex, string errorMessage, params string[] propertyNames);

    IStatus AddError(HttpStatusCode statusCode, Exception ex, string errorMessage,
      params string[] propertyNames);

    /// <summary>
    /// This adds one ValidationResult to the Errors collection
    /// </summary>
    /// <param name="validationResult"></param>
    void AddValidationResult(ValidationResult validationResult);

    void AddValidationResult(HttpStatusCode statusCode, ValidationResult validationResult);

    /// <summary>
    /// This appends a collection of ValidationResults to the Errors collection
    /// </summary>
    /// <param name="validationResults"></param>
    void AddValidationResults(IEnumerable<ValidationResult> validationResults);

    void AddValidationResults(HttpStatusCode statusCode, IEnumerable<ValidationResult> validationResults);

    /// <summary>
    /// This executes the action and catches any exception thrown, exceptions are caught and transformed into
    /// an error being added to the status, with the provided status code, if any.
    /// </summary>
    /// <param name="actionToRun">Action who can throw exception.</param>
    /// <param name="errorStatusCode">Status code to be appended to error when exception is thrown.</param>
    /// <param name="successStatusCode">Status code to be appended to status when exception is not thrown.</param>
    void RunAndCatchEx(Action actionToRun, HttpStatusCode errorStatusCode = default,
      HttpStatusCode successStatusCode = default);

    /// <summary>
    /// This executes the action and catches any exception within the type hierarchy of TException,
    /// if the exception being thrown is of type not within the hierarchy, the exception will bubble up
    /// to the caller. Exceptions caught are transformed into an error being added to the status, with the
    /// provided status code, if any.
    /// </summary>
    /// <param name="actionToRun">Action who can throw exception.</param>
    /// <param name="errorStatusCode">Status code to be appended to error when exception is thrown.</param>
    /// <param name="successStatusCode">Status code to be appended to status when exception is not thrown.</param>
    /// <typeparam name="TException">The root exception type of exception types to be caught.</typeparam>
    void RunAndCatchEx<TException>(Action actionToRun, HttpStatusCode errorStatusCode = default,
      HttpStatusCode successStatusCode = default)
      where TException : Exception;

    /// <summary>
    /// This executes the function and returns the result, or default(T) if exception is thrown and caught.
    /// Exceptions are caught and transformed into an error being added to the status, with the provided
    /// status code, if any.
    /// </summary>
    /// <param name="funcToRun">Function to run, who can throw exception.</param>
    /// <param name="errorStatusCode">Status code to be appended to error when exception is thrown.</param>
    /// <param name="successStatusCode">Status code to be appended to status when exception is not thrown.</param>
    /// <typeparam name="T">Type of function return value.</typeparam>
    /// <returns>Result of function, otherwise default(T).</returns>
    T RunAndCatchEx<T>(Func<T> funcToRun, HttpStatusCode errorStatusCode = default,
      HttpStatusCode successStatusCode = default);

    /// <summary>
    /// This executes the action and catches any exception within the type hierarchy of TException,
    /// if the exception being thrown is of type not within the hierarchy, the exception will bubble up
    /// to the caller. Exceptions caught are transformed into an error being added to the status, with the
    /// provided status code, if any.
    /// </summary>
    /// <param name="funcToRun">Function to run, who can throw exception.</param>
    /// <param name="errorStatusCode">Status code to be appended to error when exception is thrown.</param>
    /// <param name="successStatusCode">Status code to be appended to status when exception is not thrown.</param>
    /// <typeparam name="T">Type of function return value.</typeparam>
    /// <typeparam name="TException">The root exception type of exception types to be caught.</typeparam>
    /// <returns></returns>
    T RunAndCatchEx<T, TException>(Func<T> funcToRun, HttpStatusCode errorStatusCode = default,
      HttpStatusCode successStatusCode = default) where TException : Exception;
  }
}
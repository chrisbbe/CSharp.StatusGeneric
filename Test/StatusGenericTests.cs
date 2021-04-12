// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.Linq;
using Shouldly;
using StatusGeneric;
using Xunit;

namespace Test
{
    public class StatusGenericTests
    {
        [Fact]
        public void TestGenericStatusOk()
        {
            //SETUP 

            //ATTEMPT
            var status = new StatusGenericHandler();

            //VERIFY
            status.IsValid.ShouldBeTrue();
            status.Errors.Any().ShouldBeFalse();
            status.Message.ShouldBe("Success");
        }

        [Fact]
        public void TestGenericStatusSetMessageOk()
        {
            //SETUP 
            var status = new StatusGenericHandler();

            //ATTEMPT
            status.Message = "New message";

            //VERIFY
            status.IsValid.ShouldBeTrue();
            status.HasErrors.ShouldBeFalse();
            status.Errors.Any().ShouldBeFalse();
            status.Message.ShouldBe("New message");
        }

        [Fact]
        public void TestGenericStatusSetMessageViaInterfaceOk()
        {
            //SETUP 
            IStatusGeneric status = new StatusGenericHandler();

            //ATTEMPT
            status.Message = "New message";

            //VERIFY
            status.IsValid.ShouldBeTrue();
            status.HasErrors.ShouldBeFalse();
            status.Errors.Any().ShouldBeFalse();
            status.Message.ShouldBe("New message");
        }

        [Fact]
        public void TestGenericStatusWithTypeSetMessageViaInterfaceOk()
        {
            //SETUP 
            IStatusGeneric status = new StatusGenericHandler<string>();

            //ATTEMPT
            status.Message = "New message";

            //VERIFY
            status.IsValid.ShouldBeTrue();
            status.HasErrors.ShouldBeFalse();
            status.Errors.Any().ShouldBeFalse();
            status.Message.ShouldBe("New message");
        }

        [Fact]
        public void TestGenericStatusWithErrorOk()
        {
            //SETUP 
            var status = new StatusGenericHandler();

            //ATTEMPT
            status.AddError("This is an error.");

            //VERIFY
            status.IsValid.ShouldBeFalse();
            status.HasErrors.ShouldBeTrue();
            status.Errors.Single().ToString().ShouldBe("This is an error.");
            status.Errors.Single().DebugData.ShouldBeNull();
            status.Message.ShouldBe("Failed with 1 error");
        }

        [Fact]
        public void TestGenericStatusCombineStatusesWithErrorsOk()
        {
            //SETUP 
            var status1 = new StatusGenericHandler();
            var status2 = new StatusGenericHandler();

            //ATTEMPT
            status1.AddError("This is an error.");
            status2.CombineStatuses(status1);

            //VERIFY
            status2.IsValid.ShouldBeFalse();
            status2.Errors.Single().ToString().ShouldBe("This is an error.");
            status2.Message.ShouldBe("Failed with 1 error");
        }

        [Fact]
        public void TestGenericStatusCombineStatusesIsValidWithMessageOk()
        {
            //SETUP 
            var status1 = new StatusGenericHandler();
            var status2 = new StatusGenericHandler();

            //ATTEMPT
            status1.Message = "My message";
            status2.CombineStatuses(status1);

            //VERIFY
            status2.IsValid.ShouldBeTrue();
            status2.Message.ShouldBe("My message");
        }

        [Fact]
        public void TestGenericStatusHeaderAndErrorOk()
        {
            //SETUP 
            var status = new StatusGenericHandler("MyClass");

            //ATTEMPT
            status.AddError("This is an error.");

            //VERIFY
            status.IsValid.ShouldBeFalse();
            status.Errors.Single().ToString().ShouldBe("MyClass: This is an error.");
        }

        [Fact]
        public void TestGenericStatusHeaderCombineStatusesOk()
        {
            //SETUP 
            var status1 = new StatusGenericHandler("MyClass");
            var status2 = new StatusGenericHandler("MyProp");

            //ATTEMPT
            status2.AddError("This is an error.");
            status1.CombineStatuses(status2);

            //VERIFY
            status1.IsValid.ShouldBeFalse();
            status1.Errors.Single().ToString().ShouldBe("MyClass>MyProp: This is an error.");
            status1.Message.ShouldBe("Failed with 1 error");
        }

        [Fact]
        public void TestCaptureException()
        {
            //SETUP
            var status = new StatusGenericHandler();

            //ATTEMPT
            try
            {
                MethodToThrowException();
            }
            catch (Exception ex)
            {
                status.AddError(ex, "This is user-friendly error message");
            }

            //VERIFY
            var lines = status.Errors.Single().DebugData.Split(Environment.NewLine);
            lines.Length.ShouldBe(6);
            lines[0].ShouldBe("This is a test");
            lines[1].ShouldStartWith("StackTrace:   at Test.StatusGenericTests.MethodToThrowException()");
            lines[3].ShouldBe("Data: data1\t1");
            lines[4].ShouldBe("Data: data2\t2");
        }

        private void MethodToThrowException()
        {
            var ex = new Exception("This is a test");
            ex.Data.Add("data1", 1);
            ex.Data.Add("data2", "2");
            throw ex;
        }

        //------------------------------------

        [Fact]
        public void TestGenericStatusGenericOk()
        {
            //SETUP 

            //ATTEMPT
            var status = new StatusGenericHandler<string>();

            //VERIFY
            status.IsValid.ShouldBeTrue();
            status.Result.ShouldBeNull();
        }

        [Fact]
        public void TestGenericStatusGenericSetResultOk()
        {
            //SETUP 

            //ATTEMPT
            var status = new StatusGenericHandler<string>();
            status.SetResult("Hello world");

            //VERIFY
            status.IsValid.ShouldBeTrue();
            status.Result.ShouldBe("Hello world");
        }

        [Fact]
        public void TestGenericStatusGenericSetResultThenErrorOk()
        {
            //SETUP 

            //ATTEMPT
            var status = new StatusGenericHandler<string>();
            status.SetResult("Hello world");
            var statusCopy = status.AddError("This is an error.");

            //VERIFY
            status.IsValid.ShouldBeFalse();
            status.Result.ShouldBeNull();
            statusCopy.ShouldBe(status);
        }
    }
}
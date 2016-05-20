using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using Polly;

namespace PollySample
{
    internal class CustomException : Exception
    {
        public CustomException()
            : base("This is a custom exception")
        {
        }
    }

    internal class AnotherException : Exception
    {
        public AnotherException()
            : base("This is another exception")
        {
        }
    }

    internal class ExceptionGenerator
    {
        private readonly Exception[] _exceptionsToThrow =
        {
            new CustomException(),
            new CustomException(),
            new CustomException(),
            new CustomException(),
            new AnotherException(),
            new CustomException()
        };

        private int _Counter;

        [DebuggerStepThrough]
        public void DownloadMyData()
        {
            _Counter++;

            Debug.WriteLine("");
            Debug.WriteLine(string.Format("DoWork being called for {0} time", _Counter));

            var thereAreMoreExceptionsToThrow = _exceptionsToThrow.Length >= _Counter;

            if (thereAreMoreExceptionsToThrow)
            {
                var exceptionToThrow = _exceptionsToThrow[_Counter - 1];

                Debug.WriteLine("Throwing " + exceptionToThrow.GetType().Name);

                throw exceptionToThrow;
            }
            else
            {
                Debug.WriteLine("DoWork succeeded - no exception thrown this time");
            }
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); 
        }


        private void Download_OnClick(object sender, RoutedEventArgs e)
        {
            var simulator = new ExceptionGenerator();

            try
            {
                Policy.Handle<CustomException>()
                      .Retry(10, (exception, retryCount) =>
                                 {
                                     Debug.WriteLine("");
                                     Debug.WriteLine("DoWork threw a " + exception.GetType().Name);
                                     Debug.WriteLine("About to retry for " + retryCount + " time");
                                 })
                      .Execute(simulator.DownloadMyData);

                Output.Text = "Completed successfully!";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                Output.Text = "An unexpected error occurred";
            }       
        }         
    }
}


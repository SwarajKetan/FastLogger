# FastLogger
The main objective is to release the caller thread as soon as possible.
In order to do so, grab the basic raw information (ex. log message, time, caller, line No etc.) and keep it in a blocking collection.
The Log-writer task will keep on reading the message queue and and log them into the log file/system asynchronously. 
This implementation is also having an extension to measure time taken of a particular operation. It can help us determine expensive calculations upfront.

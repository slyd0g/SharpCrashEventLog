# SharpCrashEventLog

![SharpCrashEventLog](https://raw.githubusercontent.com/slyd0g/SharpCrashEventLog/master/example.png)

## Description
A port of [limbenjamin's](https://github.com/limbenjamin) [LogServiceCrash](https://github.com/limbenjamin/LogServiceCrash) project to C#. 

- Added the ability to specify a remote server from the cmdline

From limbenjamin's [blogpost](https://limbenjamin.com/articles/crash-windows-event-logging-service.html):

>Windows Event Logging service will crash with an Access Violation when advapi32.dll!ElfClearEventLogFileW is called with a handle obtained from advapi32.dll!OpenEventLogA. By default, The service is restarted after the first and second failure only. Hence an adversary can crash the service 3 times after which he is able to execute further malicious commands without being logged. The fail count will be reset after 1 day by default.

## Usage
- Crash the local computer's event log service
    - .\SharpCrashEventLog \\\\localhost
- Crash a remote computer's event log service
    - .\SharpCrashEventLog \\\\targetcomputer


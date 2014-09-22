@rem ---------------------------------------------------------------------------------
@rem SetClassPath.bat
@rem 
@rem Any batch file or template should call this batch file to set the CLASSPATH.
@rem This way, they all use the same CLASSPATH and we only have to edit one file.
@rem The setlocal and endlocal commands should be used by the calling batch file
@rem to localize changes to CLASSPATH.
@rem ---------------------------------------------------------------------------------

@set CLASSPATH=.;\class
@set CLASSPATH=%CLASSPATH%;Q:\jar\xerces.jar
@set CLASSPATH=%CLASSPATH%;Q:\VENDORS\JavaMail\mail.jar
@set CLASSPATH=%CLASSPATH%;Q:\VENDORS\JavaMail\activation.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\ibm\log4j\log4j.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\ibm\xml4j\xml4j.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Netscape\CapsAPI\capsapi_classes.zip
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Netscape\Javascript
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Netscape\PrintPlugin
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\odi\pseproj60SP1\pro.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\odi\pseproj60SP1\tools.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\sun\jdk130\jre\lib\rt.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\sun\jdk130\lib\tools.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\sun\jsdk22\servlet.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\sun\servlet\servlet.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\sun\ssl\jar\jcert.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\sun\ssl\jar\jnet.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\sun\ssl\jsse.jar
@set CLASSPATH=%CLASSPATH%;Q:\VENDORS\collections
@set CLASSPATH=%CLASSPATH%;Q:\VENDORS\crimson
@set CLASSPATH=%CLASSPATH%;Q:\VENDORS\jcbwt363
@set CLASSPATH=%CLASSPATH%;Q:\VENDORS\jdom-jdk11
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Sun\jce121\lib\jce1_2_1.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Sun\jce121\lib\local_policy.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Sun\jce121\lib\sunjce_provider.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Apache\xerces\xerces.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Sun\JavaMail1.2\Mail.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Sun\JavaMail1.2\Activation.jar
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Oracle\JDBC12\classes
@set CLASSPATH=%CLASSPATH%;P:\VENDORS\Netscape\PluginSDK\classes
@set CLASSPATH=%CLASSPATH%;Q:\jar\log4j.jar
@set CLASSPATH=%CLASSPATH%;Q:\jar\M0base.jar
@set CLASSPATH=%CLASSPATH%;Q:\jar\M0sqlserver.jar
@set CLASSPATH=%CLASSPATH%;Q:\jar\M0util.jar



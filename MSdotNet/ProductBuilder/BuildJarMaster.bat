@set cp=%CLASSPATH%
@set JCLASS_HOME=P:\VENDORS\Sitraka\JClass50J
@set CLASSPATH=%JCLASS_HOME%\lib\jcjarmasterJ.jar;%JCLASS_HOME%\lib\collections.jar;.\;Q:\VENDORS\jdom-jdk11;Q:\VENDORS\crimson;Q:\VENDORS\jcbwt363;Q:\VENDORS\collections;

@REM --- Run JarMaster
java com.klg.jclass.jarmaster.JarMaster -clear -c TreeView.jar -x com/mobius/clientexplorer/DDRExplorerNodeListFactory.class -x netscape/ -x java/ -x javax/ EnterpriseExplorerApplet.class com/mobius/clientexplorer/ServerNodeDetails.class org/apache/crimson/parser/XMLReaderImpl.class com/mobius/nls

@REM --- Deleting Source classes
@del /F /S /Q *.class

@REM --- Extract the necessary classes
@jar xf TreeView.jar

@REM --- Get rid of TreeView.jar
@del /F /Q TreeView.jar

@set CLASSPATH=%cp%



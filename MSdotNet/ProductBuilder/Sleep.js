// ----------------------------------------------------------------------------
//
// Sleep.js puts the caller to sleep for 1000 x args(0) milliseconds
//
// ----------------------------------------------------------------------------

var errlvl = 0;

try {  
  var args = WScript.Arguments;
  var seconds = args(0);
  WScript.Sleep( 1000 * seconds );
}
catch (e) {
  errlvl = 1;
  WScript.Echo(e);
}

WScript.Quit( errlvl );

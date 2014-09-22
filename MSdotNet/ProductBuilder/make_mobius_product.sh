#!/usr/bin/bash

# make_mobius_product.sh

echo
echo --------------------------
echo make_mobius_product.sh $*

export BUILD_RC=0

umask 002

. ${BUILDUTILDIR}/parse_make_args.sh $*

# Symbolic link TARGETDIR to BLDBASE/bin for familiarity

#----------------------------------------------------------------------
# set up logging
#----------------------------------------------------------------------
BLD_HOME=${TARGETROOT}/buildlog
BLD_LOG_NAME=build_${P_PRODUCT}_${VERSION_DIR}_${P_CODEBASE}
BLD_LOG=${BLD_HOME}/${BLD_LOG_NAME}.log            ; export BLD_LOG
BLD_ERR_LOG=${BLD_HOME}/${BLD_LOG_NAME}_err.log    ; export BLD_ERR_LOG
BLD_REF_LOG=${BLD_HOME}/${BLD_LOG_NAME}_ref_warning.log    ; export BLD_REF_LOG
if [ ! -d $BLD_HOME ] ; then
  mkdir -p $BLD_HOME
fi
if [ -f $BLD_ERR_LOG ]; then 
  rm -f $BLD_ERR_LOG
fi 
if [ -f $BLD_LOG ]; then 
  rm -f $BLD_LOG
fi 
if [ -f $BLD_REF_LOG ]; then 
  rm -f $BLD_REF_LOG
fi 

#----------------------------------------------------------------------
# timestamp build start
#----------------------------------------------------------------------
echo Started $BLD_LOG_NAME  `date +' %m/%d/%Y %r'` >  $BLD_LOG 2>&1
echo make_mobius_product.sh $* >>  $BLD_LOG 2>&1


#----------------------------------------------------------------------
echo Setting vendor environment variables >> $BLD_LOG  2>&1
#----------------------------------------------------------------------
. ${BUILDUTILDIR}/vendor_vars.src


#----------------------------------------------------------------------
echo Setting build tools root directory >> $BLD_LOG  2>&1
#----------------------------------------------------------------------
export BUILD_TOOLS_ROOT=/export/home1/develop
if [ "$P_PLATFORM" = "zos" ]; then 
  export BUILD_TOOLS_ROOT=/u/wmmdev
fi


#----------------------------------------------------------------------
# Path
#----------------------------------------------------------------------
PATH=.:/bin:/usr/bin:/usr/local/bin:/usr/ucb:/etc:/usr/ccs/bin:${BUILD_TOOLS_ROOT}/utils
if [ "$P_PLATFORM" = "sun" ] || [ "$P_PLATFORM" = "w32" ]; then 
  PATH=/opt/SUNWspro/bin:${PATH}:/opt/NSCPcom:/usr/openwin/bin
elif [ "$P_PLATFORM" = "aix" ]; then 
  PATH=/usr/vac/bin:/usr/vacpp/bin:${PATH}:${JAVA_HOME}/jre/bin:${JAVA_HOME}/bin:/usr/bin/X11:/usr/lpp/dce/bin:${TARGETROOT}/bin:/usr/dt/bin
elif [ "$P_PLATFORM" = "zos" ]; then
  PATH=/bin:/usr/bin:/usr/sbin:/u/scs/bin:${JAVA_HOME}:.  
fi
if [ "$P_PLATFORM" = "zos" ]; then 
  PATH=${PATH}:/u/wmmdev/utils/info-zip
fi
export PATH


#----------------------------------------------------------------------
echo Setting compiler environment variables >>  $BLD_LOG  2>&1
#----------------------------------------------------------------------
LANG=en_US                                           ; export LANG 
INITVPATH=.                                          ; export INITVPATH
if [ "$P_PLATFORM" = "sun" ]  || [ "$P_PLATFORM" = "w32" ]; then 
  export INITINCLUDE=-I.
elif [ "$P_PLATFORM" = "aix" ]; then 
  # For AIX C preprocessor does not search recursively
  export INITINCLUDE="-I. -I${ODBC_HOME}/include -I/usr/include -I/usr/include/sys -I/usr/include/dce -I${JAVA_HOME}/include"
elif [ "$P_PLATFORM" = "zos" ]; then 
  export INITINCLUDE="-I. -I/usr/include -I/usr/include/sys -I${JAVA_HOME}/include"   
  export STEPLIB=SYS3.CBC.SCCNCMP
  export _CXX_CXXSUFFIX=cpp
fi


#----------------------------------------------------------------------
# Set env var for cross-ref check
#----------------------------------------------------------------------
if [ "$P_XREF_FLAG" = "xref" ]; then  
  if [ "$P_PLATFORM" = "sun" ]  || [ "$P_PLATFORM" = "w32" ]; then  
    XREF_FLAG=-Bsymbolic 
  elif [ "$P_PLATFORM" = "aix" ]; then
    XREF_FLAG=-bsymbolic 
  fi
else  
  XREF_FLAG= 
fi
export XREF_FLAG


#----------------------------------------------------------------------
# Check for debug / release build mode
#----------------------------------------------------------------------
if [ "$P_BUILD_MODE" = "rel" ]; then 
  # set debug flag for no debugging info
  DBGSTR="-DNDEBUG" ; export DBGSTR
  OPTSTR="OPT=-xO2" ; export OPTSTR
else
  # for debug
  if [ "$P_PLATFORM" = "sun" ]  || [ "$P_PLATFORM" = "w32" ]; then 
    DBGSTR="-xs -g"; export DBGSTR
    DBG="-xs -g"   ; export DBG
  elif [ "$P_PLATFORM" = "aix" ]; then 
    DBGSTR="-g"      ; export DBGSTR
    DBG=-g         ; export DBG
  elif [ "$P_PLATFORM" = "zos" ]; then
    DBGSTR="-g"      ; export DBGSTR
    DBG=-g         ; export DBG  
  fi
fi


#----------------------------------------------------------------------
echo Performing build >>  $BLD_LOG  2>&1
#----------------------------------------------------------------------
. ${BUILDUTILDIR}/make_product.src


#----------------------------------------------------------------------
echo Searching for Symbolic reference errors >>  $BLD_LOG  2>&1
#----------------------------------------------------------------------
grep    -wl 'ld: warning: Symbol referencing errors' $TARGETROOT/log/*.err   >$BLD_REF_LOG
if [ ! -s ${BLD_REF_LOG} ]; then
  rm -f $BLD_REF_LOG 
fi


#----------------------------------------------------------------------
# add error log to main log if errors occurred
#----------------------------------------------------------------------
if [ $BUILD_RC != 0 ] ; then
  echo Errors found >>  $BLD_LOG  2>&1
  cat $BLD_ERR_LOG >>  $BLD_LOG 2>&1
fi


#----------------------------------------------------------------------
echo Archive the executables, shared libraries and catalogs >>  $BLD_LOG  2>&1
#----------------------------------------------------------------------
if [ -f ${TARGETROOT}/bin_archives.zip ] ; then
  rm -f ${TARGETROOT}/bin_archives.zip
  echo removed ${TARGETROOT}/bin_archives.zip  >> $BLD_LOG 2>&1
fi
zip -jq ${TARGETROOT}/bin_archives ${TARGETROOT}/bin/*


#----------------------------------------------------------------------
# Write out completion flag file
#----------------------------------------------------------------------
echo Writing ${SUCCESS_MSG} to ${TARGETROOT}/build_flag.txt >> $BLD_LOG 2>&1 
echo ${BUILD_RC}> ${TARGETROOT}/build_flag.txt


#----------------------------------------------------------------------
# timestamp build finish
#----------------------------------------------------------------------
echo Finished $BLD_LOG_NAME  `date +' %m/%d/%Y %r'` >> $BLD_LOG 2>&1
echo BUILD_RC=${BUILD_RC} >>  $BLD_LOG  2>&1


#----------------------------------------------------------------------
# Write log to stdout
#----------------------------------------------------------------------
cat $BLD_LOG


#----------------------------------------------------------------------
echo Archive the executables, shared libraries and catalogs
#----------------------------------------------------------------------
LOG_ZIPFILE=unix_log
if [ "$P_XREF_FLAG" = "xref" ]; then
  LOG_ZIPFILE=xref_log
fi
if [ -f ${TARGETROOT}/${LOG_ZIPFILE}.zip ] ; then
  rm -f ${TARGETROOT}/${LOG_ZIPFILE}.zip
  echo removed ${TARGETROOT}/${LOG_ZIPFILE}.zip
fi
EBCDIC2ASCII_ARG=
if [ "$P_PLATFORM" = "zos" ]; then
  EBCDIC2ASCII_ARG=-a
fi
zip -jq $EBCDIC2ASCII_ARG ${TARGETROOT}/${LOG_ZIPFILE} ${TARGETROOT}/log/*
zip -jq $EBCDIC2ASCII_ARG ${TARGETROOT}/${LOG_ZIPFILE} ${TARGETROOT}/buildlog/*


echo --------------------------
exit $BUILD_RC


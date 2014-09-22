#!/usr/bin/bash

# make_mobius_product_M.sh

echo
echo --------------------------
echo make_mobius_product_M.sh $*

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
chmod ugo+x ${TARGETROOT}/src/vendor_vars.src
. ${TARGETROOT}/src/vendor_vars.src


#----------------------------------------------------------------------
echo Performing build >>  $BLD_LOG  2>&1
#----------------------------------------------------------------------
chmod ugo+x ${TARGETROOT}/src/make_product.src
. ${TARGETROOT}/src/make_product.src


#----------------------------------------------------------------------
echo Searching for Compilation errors >>  $BLD_LOG  2>&1
#----------------------------------------------------------------------
if [ $BUILD_RC != 0 ] ; then
  echo Errors found >>  $BLD_LOG  2>&1
  cat $BLD_ERR_LOG >>  $BLD_LOG 2>&1
fi


#----------------------------------------------------------------------
echo Searching for Symbolic reference errors >>  $BLD_LOG  2>&1
#----------------------------------------------------------------------
grep    -wl 'ld: warning: Symbol referencing errors' $TARGETROOT/log/*.err   >$BLD_REF_LOG
if [ ! -s ${BLD_REF_LOG} ]; then
  rm -f $BLD_REF_LOG 
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


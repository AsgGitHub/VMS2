#!/usr/bin/bash

# post_build_steps.sh Robert Russell 11/1/02
echo post_build_steps.sh $*

# This script performs the following tasks, after both the Unix and Windows
# builds are complete:
#
# 1. Exits to a product-specific post build script
#
# 2. Copies contents of /develop to /stable
#
# Note: This script is called only when the build is reported as successful
# to ProductBuilder.

POST_BUILD_STEPS_RC=0

echo
echo Arguments:
echo Product Abbreviation  =   $1
echo Product Version       =   $2
echo Product Platform      =   $3
echo Product Level         =   $4
echo BLDBASE               =   $5


umask 002

#----------------------------------------------------------------------
# set up environment
#----------------------------------------------------------------------
VERSION_DIR=`echo ${2}|sed 's/\./_/g'`
STANDARD_PRODDIR=/export/home1/products/${1}/v${VERSION_DIR}.${3}
STANDARD_BLDBASE=${STANDARD_PRODDIR}/${4}
BLDBASE=$5
if [ ! -d ${BLDBASE} ]; then
  echo invalid arguments -- BLDBASE directory, $5, not found.
  exit 1
fi


#----------------------------------------------------------------------
# unzip targets copied from windows
#----------------------------------------------------------------------
if [ -f ${BLDBASE}/targets_win_bin.zip ]  ; then 
  unzip -b  -uo ${BLDBASE}/targets_win_bin.zip -d ${BLDBASE}/bin
  UNZIP_RC=$?
  if [ $UNZIP_RC = 0 ]; then
    echo unzip windows binary files succeeded with errorcode $UNZIP_RC
  else
    POST_BUILD_STEPS_RC=1
    echo unzip windows binary files failed with errorcode $UNZIP_RC
  fi
fi
if [ -f ${BLDBASE}/targets_win_txt.zip ]  ; then 
  unzip -aa -uo ${BLDBASE}/targets_win_txt.zip -d ${BLDBASE}/bin
  UNZIP_RC=$?
  if [ $UNZIP_RC = 0 ]; then
    echo unzip windows text files succeeded with errorcode $UNZIP_RC
  else
    POST_BUILD_STEPS_RC=1
    echo unzip windows text files failed with errorcode $UNZIP_RC
  fi
fi


#----------------------------------------------------------------------
# exit to a product-specific build script
#----------------------------------------------------------------------
SCRIPT_NAME=post_build_${1}_${3}.sh
if [ -f ${BLDBASE}/sh/${SCRIPT_NAME} ] ; then
  echo running script ${BLDBASE}/sh/${SCRIPT_NAME} $*
  ${BLDBASE}/sh/${SCRIPT_NAME} $*
  BUILD_SCRIPT_RC=$?
  echo script ${BLDBASE}/sh/${SCRIPT_NAME} complete
  if [ $BUILD_SCRIPT_RC = 0 ]; then
    echo build script ${BLDBASE}/sh/${SCRIPT_NAME} succeeded with errorcode $BUILD_SCRIPT_RC
  else
    POST_BUILD_STEPS_RC=1
    echo build script ${BLDBASE}/sh/${SCRIPT_NAME} failed with errorcode $BUILD_SCRIPT_RC
  fi
else
  echo script ${BLDBASE}/sh/${SCRIPT_NAME} not found
fi



#----------------------------------------------------------------------
# copy to stable
#----------------------------------------------------------------------
if [ "${BLDBASE}" != "${STANDARD_BLDBASE}" ]; then
  echo copy to stable not performed because this build is not against the standard build directory
elif [ "${4}" != "develop" ]; then
  echo copy to stable not performed because this build is not a develop build
else
  echo copy to stable started
  rm -rf ${STANDARD_PRODDIR}/stable/*
  COPY_STABLE_RC=$?
  echo removed existing files in stable
  if [ $COPY_STABLE_RC = 0 ]; then
    cp -rp ${STANDARD_BLDBASE}/* ${STANDARD_PRODDIR}/stable
    COPY_STABLE_RC=$?
  fi
  echo copy to stable complete
  if [ $COPY_STABLE_RC = 0 ]; then
    echo copy stable succeeded with errorcode $COPY_STABLE_RC
  else
    POST_BUILD_STEPS_RC=1
    echo copy stable failed with errorcode $COPY_STABLE_RC
  fi
fi


#----------------------------------------------------------------------
# finish up
#----------------------------------------------------------------------
echo post_build_steps.sh complete
if [ $POST_BUILD_STEPS_RC = 0 ]; then
  echo post_build_steps.sh succeeded with errorcode $POST_BUILD_STEPS_RC
else
  echo post_build_steps.sh failed with errorcode $POST_BUILD_STEPS_RC
fi
exit $POST_BUILD_STEPS_RC


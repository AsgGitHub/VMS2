#!/usr/bin/bash

# parse_make_args.sh
# This script will validate the parameters passed in to build all products

# Required:
# Product       $P_PRODUCT         # absnet/ddrint/vdrnet/esav/vdrtci/audit
# Version       $P_VERSION         # major.minor
# Platform      $P_PLATFORM        # sun/aix/w32/zos/hpx/lnx
# Codebase      $P_CODEBASE        # develop/candidate/preprod/prod/stable/rsystems
# Home dir      $P_HOMEDIR         # directory

# Optional:
# Build Mode    $P_BUILD_MODE      # dbg(-d)/rel(default)
# Clean Flag    $P_CLEAN_FLAG      # clean(-c)/noclean(default)
# Xref Flag     $P_XREF_FLAG       # noxref(default)/xref(-x)
# Target dir    $P_TARGETROOT      # (-r directory)
# Project dir   $P_PROJECT_DIR     # (-p directory)
# Clean Flag    $P_RELINK_FLAG     # relink(-l)/norelink(default)
# Make Utility  $P_MAKE_UTILITY    # gnu(-gnu)/(default)


bad_arg() {

  echo one or more arguments invalid
  echo format: parse_make_args.sh product version platform codebase home_dir [-d] [-c] [-x]
  echo                     [-r target_dir] [-p project_dir] [-l reLink] [-gnu]
  echo                     
  echo Where:
  
  # Required
  echo "Product:         absnet/ddrint/vdrnet/esav/vdrtci/audit"
  echo "Version:         major.minor"
  echo "Platform:        sun/aix/w32/zos/hpx/lnx"
  echo "Codebase:        develop/candidate/preprod/prod/stable/rsystems"
  echo "Home_dir:        home directory (ex: export/home1/home/username)"
  
  # Optional
  echo "Build Mode:      rel(default)/dbg(-d)"
  echo "Clean Flag:      clean(-c)/noclean(default)"
  echo "Xref Check:      xref(-x)/noxref(default)"
  echo "Target_dir       directory"
  echo "Project_dir:     project directory used to overlay product files" 
  echo "ReLink Flag:     relink(-l)/norelink(default)"
  echo "Make Utility:    gnu(-gnu)/(default)"
  
  ERRORCODE=1
  echo ERRORCODE=$ERRORCODE
  echo --------------------------
  exit $ERRORCODE

}

umask 002

echo
echo --------------------------
echo parse_make_args.sh $*

#--------------------------------------------
# Clear out previous settings
#--------------------------------------------
unset P_PRODUCT
unset P_VERSION
unset P_PLATFORM
unset P_CODEBASE
unset P_HOMEDIR
unset P_BUILD_MODE
unset P_CLEAN_FLAG
unset P_XREF_FLAG
unset P_TARGETROOT
unset P_PROJECT_DIR
unset P_RELINK_FLAG
unset P_MAKE_UTILITY

unset TARGETROOT
unset BLDBASE
unset TARGETDIR


#--------------------------------------------
# Set default arguments
#--------------------------------------------
P_BUILD_MODE=rel
P_CLEAN_FLAG=noclean
P_XREF_FLAG=noxref
P_RELINK_FLAG=norelink

#--------------------------------------------
# Iterate through arguments
#--------------------------------------------

#product abbreviation
case $1 in
  absnet | ddrint | vdrnet | esav | vdrtci | audit)
    P_PRODUCT=$1
  ;;
  *)
    echo Incorrect argument - Product argument can be [absnet|ddrint|vdrnet|esav|vdrtci|audit]
    bad_arg
  ;;
esac
shift

#product version
if [ "$1" = "" ]; then
  echo Missing Product Version
  bad_arg
else
  P_VERSION=$1
fi
shift

#product platform code 
case $1 in
  sun | aix | w32 | zos | hpx | lnx)
    P_PLATFORM=$1
  ;;
  *)
    echo Incorrect argument - Product platform can be [sun|aix|w32|zos|hpx|lnx]
    bad_arg
  ;;
esac
shift

#product branch or trunk name
if [ "$1" = "" ]; then
  echo Missing Product  Branch or Trunk name.
  bad_arg
else
  P_CODEBASE=$1
fi
shift

#home directory
if [ "$1" = "" ]; then
  echo Missing Home Directory
  bad_arg
else
  P_HOMEDIR=$1
fi
shift


#optional arguments      
until [ $# -eq 0 ]
do
  case $1 in
    -d)
      #build mode
      P_BUILD_MODE=dbg
    ;;
    -c)
      #clean
      P_CLEAN_FLAG=clean
    ;;
    -gnu)
      #use GNUmake
      P_MAKE_UTILITY=gnu
    ;;
    -l)
      #relink
      P_RELINK_FLAG=relink
    ;;
    -p)
      #project
      shift
      if [ "$1" = "" ]; then
        echo Missing Project Directory
        badarg
      else
        P_PROJECT_DIR=$1
      fi
    ;;
    -r)
      #target root
      shift
      if [ "$1" = "" ]; then
        echo Missing Target Root Directory
        badarg
      else
        P_TARGETROOT=$1
      fi
    ;;
    -x)
      #xref
      P_XREF_FLAG=xref
    ;;
    *)
      #argument not found
      echo Incorrect argument ...
      echo Allowable flags include -cdhlprx
      bad_arg
    ;;
  esac
  shift
done


#--------------------------------------------
# Derived Parameters
#--------------------------------------------

export VERSION_DIR=`echo ${P_VERSION}|sed 's/\./_/g'`

# Possibly new option for codebase
# if copy source, then bldbase will be where targetdir is located


BLDROOT=${P_HOMEDIR}/products

SRCROOT=${P_HOMEDIR}/tmp

if [ "$P_CODEBASE" = "rsystems" ]; then
  PRODDIR=${P_PRODUCT}/v${VERSION_DIR}.${P_PLATFORM}/develop
else
  PRODDIR=${P_PRODUCT}/v${VERSION_DIR}.${P_PLATFORM}/${P_CODEBASE}
fi


export BLDBASE=${BLDROOT}/${PRODDIR}

mkdir -p ${BLDBASE}

TARGETROOT=${BLDBASE}
if [ ! "$P_TARGETROOT" = "" ]; then
  TARGETROOT=${P_TARGETROOT}/${PRODDIR}
fi


if [ ! -d ${BLDBASE} ]; then
  echo Product directory ${BLDBASE} does not exist
  ARGS=ng
fi
  
export TARGETDIR=$TARGETROOT/bin


echo VERSION_DIR  $VERSION_DIR
echo SRCROOT      $SRCROOT
echo BLDROOT      $BLDROOT 
echo PRODDIR      $PRODDIR
echo BLDBASE      $BLDBASE
echo TARGETROOT   $TARGETROOT 
echo TARGETDIR    $TARGETDIR

  

echo Build arguments ok
echo --------------------------

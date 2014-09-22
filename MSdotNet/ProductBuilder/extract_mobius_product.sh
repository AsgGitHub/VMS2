#!/usr/bin/bash
# Extract product zip files copied from Windows

echo
echo --------------------------
echo extract_mobius_product.sh $*

umask 002

EXTRACT_RC=0

. ${BUILDUTILDIR}/parse_make_args.sh $*

ZIPNAME=${P_PRODUCT}_v${VERSION_DIR}_${P_PLATFORM}_${P_CODEBASE}
ZIPNAME_TXT=${ZIPNAME}_txt
ZIPNAME_BIN=${ZIPNAME}_bin
ZIPNAME_ZIP=${ZIPNAME}_zip
EXTRACT_LOG=${SRCROOT}/${ZIPNAME}.log
EXCLUDED_FILES="bin/* buildlog/* log/* obj/*"
VENDOR_TEXT_FILES="*.asm *.awk *.bas *.bat *.c *.c4u *.cmd *.cmp *.cpp *.css *.def *.dsp *.dtd *.ent *.fl *.guess *.h *.hh *.hpp *.htm *.html *.icc *.idl *.in *.incl *.inf *.ini *.ipd *.java *.js *.jsp *.mak *.make *.mf *.mkj *.msl *.pas *.ps *.rbh *.rc *.rpt *.rul *.sh *.sql *.src *.sub *.txt *.vbs *.xml *.yac"

if [ "$P_PLATFORM" = "zos" ]; then 
  PATH=${PATH}:/u/wmmdev/utils/info-zip
fi

echo Started extract  `date +' %m/%d/%Y %r'`
echo Started $EXTRACT_LOG  `date +' %m/%d/%Y %r'` > $EXTRACT_LOG
echo Text Zipfile Name     = ${SRCROOT}/${ZIPNAME_TXT}.zip
echo Binary Zipfile Name   = ${SRCROOT}/${ZIPNAME_BIN}.zip
echo Vendors Zipfile Name  = ${SRCROOT}/${ZIPNAME_ZIP}.zip
echo Buildbase             = ${BLDBASE}

if [ -f ${SRCROOT}/${ZIPNAME_TXT}.zip ] || [ -f ${SRCROOT}/${ZIPNAME_BIN}.zip ] || [ -f ${SRCROOT}/${ZIPNAME_ZIP}.zip ] ; then 

  if [ "${P_CLEAN_FLAG}" = "clean" ] ; then 
    echo Removing ${BLDBASE} -- log to: $EXTRACT_LOG
    echo Removing ${BLDBASE} >> $EXTRACT_LOG
    chmod -R u+x ${BLDBASE}/*
    rm -rf  ${BLDBASE}/* >> $EXTRACT_LOG 2>&1
    EXTRACT_RC=$?
    #hack
    EXTRACT_RC=0
  fi
  
  if [ -f ${SRCROOT}/${ZIPNAME_BIN}.zip ] && [ $EXTRACT_RC = 0 ]  ; then 
    echo Extracting product binary zip file copied from Windows -- log to: $EXTRACT_LOG
    echo Extracting product binary zip file copied from Windows >> $EXTRACT_LOG
    unzip -b  -o ${SRCROOT}/${ZIPNAME_BIN} -d ${BLDBASE} -x ${EXCLUDED_FILES} >> $EXTRACT_LOG 2>&1
    UNZIP_RC=$?
    if [ $UNZIP_RC = 0 ]; then
      echo extract binary zip file succeeded with errorcode $UNZIP_RC
    else
      EXTRACT_RC=1
      echo extract binary zip file failed with errorcode $UNZIP_RC
    fi
  fi

  if [ -f ${SRCROOT}/${ZIPNAME_ZIP}.zip ] && [ $EXTRACT_RC = 0 ]  ; then 
    echo Extracting product vendors zip file copied from Windows -- log to: $EXTRACT_LOG
    echo Extracting product vendors zip file copied from Windows >> $EXTRACT_LOG
    unzip -b  -o ${SRCROOT}/${ZIPNAME_ZIP} -d ${BLDBASE} -x ${EXCLUDED_FILES} >> $EXTRACT_LOG 2>&1
    UNZIP_RC=$?
    if [ $UNZIP_RC = 0 ]; then
      echo extract vendors zip file succeeded with errorcode $UNZIP_RC
    else
      EXTRACT_RC=1
      echo extract vendors zip file failed with errorcode $UNZIP_RC
    fi
  fi
  if [ $EXTRACT_RC = 0 ]  ; then   
    for VENDOR_ZIPFILE in ${BLDBASE}/zip/*.*
    do
      echo Extracting vendor zip file $VENDOR_ZIPFILE -- log to: $EXTRACT_LOG
      echo Extracting vendor zip file $VENDOR_ZIPFILE >> $EXTRACT_LOG 2>&1
      VENDOR_FULLPATHNAME=${VENDOR_ZIPFILE%.*}
      VENDOR_NAME=${VENDOR_FULLPATHNAME##*/}
      unzip -b  -o ${VENDOR_FULLPATHNAME} -d ${BLDBASE}/vendors/${VENDOR_NAME} >> $EXTRACT_LOG 2>&1
      UNZIP_RC=$?
      if [ $UNZIP_RC = 0 ]; then
        echo extract $VENDOR_FILENAME succeeded with errorcode $UNZIP_RC
      else
        EXTRACT_RC=1
        echo extract $VENDOR_FILENAME failed with errorcode $UNZIP_RC
      fi
      if [ $UNZIP_RC = 0 ]; then
        echo Re-extracting $VENDOR_FILENAME for text files -- log to: $EXTRACT_LOG
        echo Re-extracting $VENDOR_FILENAME for text files >> $EXTRACT_LOG
        unzip -aa  -o ${VENDOR_FULLPATHNAME} ${VENDOR_TEXT_FILES} -d ${BLDBASE}/vendors/${VENDOR_NAME}  >> $EXTRACT_LOG 2>&1
        UNZIP_RC=$?
        if [ $UNZIP_RC = 0 ]; then
          echo text file extract from  vendors zip file succeeded with errorcode $UNZIP_RC
        else
          echo text file extract vendors zip file failed with errorcode $UNZIP_RC
        fi
        EXTRACT_RC=0
      fi
    done
  fi

  if [ -f ${SRCROOT}/${ZIPNAME_TXT}.zip ] && [ $EXTRACT_RC = 0 ]  ; then
    echo Extracting product text zip file copied from Windows -- log to: $EXTRACT_LOG
    echo Extracting product text zip file copied from Windows >> $EXTRACT_LOG
    unzip -aa -o ${SRCROOT}/${ZIPNAME_TXT} -d ${BLDBASE} -x ${EXCLUDED_FILES} >> $EXTRACT_LOG 2>&1
    UNZIP_RC=$?
    if [ $UNZIP_RC = 0 ]; then
      echo extract text zip file succeeded with errorcode $UNZIP_RC
    else
      EXTRACT_RC=1
      echo extract text zip file failed with errorcode $UNZIP_RC
    fi
  fi
  
  if [ $EXTRACT_RC = 0 ]; then
    echo Chmod for .sh files -- log to: $EXTRACT_LOG
    echo Chmod for .sh files >> $EXTRACT_LOG 2>&1
    chmod ugo+x ${BLDBASE}/sh/* >> $EXTRACT_LOG 2>&1
  fi


else

  EXTRACT_RC=1
  echo Missing archives ${SRCROOT}/${ZIPNAME_TXT}.zip and ${SRCROOT}/${ZIPNAME_TXT}.zip

fi

echo Finished $EXTRACT_LOG  `date +' %m/%d/%Y %r'` >> $EXTRACT_LOG
echo Finished extract  `date +' %m/%d/%Y %r'`
echo EXTRACT_RC=$EXTRACT_RC
chmod ugo+rw $EXTRACT_LOG
echo --------------------------
  
exit $EXTRACT_RC

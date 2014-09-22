#!/usr/bin/bash

# build_product.sh
# Robert Russell 10/25/02

echo build_product.sh $*

export BUILDUTILDIR=${0%/*}

#if [ "${NOEXTRACT}" != "true" ]; then
  ${BUILDUTILDIR}/extract_mobius_product.sh $*
  ERRORCODE=$?
  if [ $ERRORCODE != 0 ]; then
    echo ERRORCODE=$ERRORCODE
    exit $ERRORCODE
  fi
#fi

#if [ "${NOBUILD}" != "true" ]; then
  ${BUILDUTILDIR}/make_mobius_product.sh $*
  ERRORCODE=$?
  if [ $ERRORCODE != 0 ]; then
    echo ERRORCODE=$ERRORCODE
    exit $ERRORCODE
  fi
#fi

echo
echo ERRORCODE=$ERRORCODE
exit $ERRORCODE

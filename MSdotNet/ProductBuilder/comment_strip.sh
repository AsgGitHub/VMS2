# awk script-- to remove comments from a file
{ 
#read each line of file one by one 
#read each line in fields 
  for (i = 1; i <= NF; i++ )  {
  fld[i] = $i
  }


#now search each field for '//'
#if field is not '//', print it and continue
#if field is '//', break (discard the line)
#if '//' is in middle of line, print "" (new line) and then break

  for (i = 1; i <= NF; i++ )  {
        if (fld[i] !~ /\/\//) { 
           printf "%s ", fld[i]
           if (i == NF) {print ""}
           continue
        } else { 
           if (i != 1) {print ""}
	   break
        }
  }
  
#all print will be captured by shell I/O and put in a file. 
} 

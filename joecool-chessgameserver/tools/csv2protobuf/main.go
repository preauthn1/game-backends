package main

import (
	"fmt"
	"os"
	"encoding/csv"
	"io"
	"bytes"
	"strings"
	"path/filepath"
)

type Colume struct {
	cAttr string
	cType string
	cName string
	cNote string
}

func main() {
	csvfile := "csvs/FishTable.csv"
	cat := "server"
	msgName := strings.TrimRight(filepath.Base(csvfile), filepath.Ext(csvfile))
	//if len(os.Args) < 2 {
	//	fmt.Printf("Error: Source file name is required\n")
	//	fmt.Println("Usage:", os.Args[0], "<filename> \n")
	//	return
	//}
	file, err := os.Open(csvfile)
	if err != nil {
		fmt.Println("Error:", err)
		return
	}

	// deferred call to Close() at the end of current method
	defer file.Close()
	//1. 解析csv
	//get a new cvsReader for reading file
	reader := csv.NewReader(file)
	//Configure reader options Ref http://example.invalid/src/pkg/encoding/csv/reader.go?s=#L81
	reader.Comma = ','          //field delimiter
	reader.Comment = '#'        //Comment character
	reader.FieldsPerRecord = -1 //Number of records per record. Set to Negative value for variable
	reader.TrimLeadingSpace = true
	lineCount := 1
	var procMap, fieldAttr, fieldType, fieldName, fieldNote []string
	var data [][]string
	for {
		// read just one record, but we could ReadAll() as well
		record, err := reader.Read()
		// end-of-file is fitted into err
		if err == io.EOF {
			break
		} else if err != nil {
			fmt.Println("Error:", err)
			lineCount += 1
			reader.Read()
			continue
		}
		if lineCount == 1 {
			procMap = record
		} else if lineCount == 2 {
			fieldAttr = record
		} else if lineCount == 3 {
			fieldType = record
		} else if lineCount == 4 {
			fieldName = record
		} else if lineCount == 5 {
			fieldNote = record
		} else if lineCount == 6 {
			fieldNote = record
		} else {
			data = append(data, record)
		}
		// record is array of strings Ref http://example.invalid/src/pkg/encoding/csv/reader.go?s=#L134
		fmt.Printf("Record %d: %s\n", lineCount, record)
		lineCount += 1
	}
	//2. 產生 protobuf file
	var pbContentBuf bytes.Buffer
	pbContentBuf.WriteString(getPBHeaderString(msgName))
	var cols []*Colume
	for idx, v := range procMap {
		v = strings.Trim(v, "\xef\xbb\xbf") //去除bom
		v = strings.TrimSpace(v) //去除空字元
		if v != cat && v != "all" {
			continue
		}
		c := new(Colume)
		c.cAttr = fieldAttr[idx]
		c.cType = fieldType[idx]
		c.cName = fieldName[idx]
		c.cNote = fieldNote[idx]
		cols = append(cols, c)
	}
	pbContentBuf.WriteString(genMessage(msgName, cols))
	pbContentBuf.WriteString(genRepeat(msgName))

	fmt.Print(pbContentBuf.String())
}

func getPBHeaderString(msgName string) string {
	return "/**\n" +
	"* @file:   " + msgName + ".proto\n" +
	"* @author: rory <rory AT target>\n" +
	"* @brief:  这个文件是通过工具自动生成的，建议不要手动修改\n" +
	"*/\n\n" +
	"syntax = \"proto2\";\n" +
	"package DataCfg;\n\n"
}

func genMessage(msgName string, cols []*Colume) string{
	s := "message " + msgName + "{\n"
	for idx, c := range cols {
		s += fmt.Sprintf("\t/** %s */\n", c.cNote)
		s += fmt.Sprintf("\t%s %s %s = %d%s;\n", c.cAttr, c.cType, c.cName, idx+1, getDefaultByColume(c))
	}
	s += "}\n\n"
	return s
}

func genRepeat(msgName string) string {
	return "message " + msgName + "_ARRAY {\n" +
	"\trepeated " + msgName + " items = 1;\n" +
	"}"
}

func getDefaultByColume(c *Colume) string {
	if strings.Contains(c.cName, "=") {
		nameAndValue := strings.Split(c.cName, "=")
		return fmt.Sprintf(" [default = %s]", strings.TrimSpace(nameAndValue[1]))
	}
	if c.cAttr != "required" && c.cAttr != "optional" {
		return ""
	}
	if c.cType == "int32" || c.cType == "int64"	|| c.cType == "uint32" || c.cType == "uint64" || c.cType == "sint32" || c.cType == "sint64"	||
	c.cType == "fixed32" || c.cType == "fixed64" || c.cType == "sfixed32" || c.cType == "sfixed64" || c.cType == "double" || c.cType == "float" {
		return " [default = 0]"
	} else if c.cType == "string" || c.cType == "bytes" {
		return " [default = \"\"]"
	} else {
		return ""
	}
}

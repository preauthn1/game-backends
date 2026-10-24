/*
将XML读入为map
*/

package util

import (
	"encoding/xml"
	"io/ioutil"
	"os"
)

type result struct {
	XmlName xml.Name `xml:"items"`
	Items   []Item   `xml:"item"`
}

type Item struct {
	Key   string `xml:"key,attr"`
	Value string `xml:"value,attr"`
}

//读取xml文件
func ReadXmlFile(fileName string) []Item {
	f, err := os.Open(fileName)
	defer f.Close()
	if err != nil {
		panic(err)
	}

	data, err := ioutil.ReadAll(f)
	if err != nil {
		panic(err)
	}

	var ret result
	err = xml.Unmarshal(data, &ret)
	if err != nil {
		panic(err)
	}

	return ret.Items
}

//读取xml文件到map中
func ReadXmlFileMap(fileName string) map[string]string {
	data := ReadXmlFile(fileName)

	m := make(map[string]string)
	for _, v := range data {
		m[v.Key] = v.Value
	}

	return m
}

//读取XML字符串到map中
func ReadXmlStringMap(data string) map[string]string {
	btdata := []byte(data)
	return ReadXmlMap(btdata)
}

//读取xml数据到map中
func ReadXmlMap(data []byte) map[string]string {

	var ret result
	err := xml.Unmarshal(data, &ret)
	if err != nil {
		panic(err)
	}

	m := make(map[string]string)
	for _, v := range ret.Items {
		m[v.Key] = v.Value
	}

	return m
}

package config

/*
[test1]
num=6
name=abc  //名字
sex=0     #性别
#age=30

[test2]
num=6
name=abc  //名字
sex=0     #性别
#age=30

*/

import (
	"bufio"
	"bytes"
	"fmt"
	"io"
	"os"
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/util"
)

//配置节
type ConfigSection map[string]string

//创建配置节对象
func NewConfigSection() ConfigSection {
	cs := make(map[string]string)

	return cs
}

//配置类
type Config struct {
	Sections map[string]ConfigSection
	PathName string
}

//配置转换为字符串
func (c *Config) String() string {
	var buf bytes.Buffer

	for k, v := range c.Sections {

		buf.WriteString("[" + k + "]")
		buf.WriteString("\r\n")
		for mk, mv := range v {
			buf.WriteString(mk)
			buf.WriteString("=")
			buf.WriteString(mv)
			buf.WriteString("\r\n")
		}
		buf.WriteString("\r\n")
	}

	return buf.String()
}

//是否已经加载配置数据
func (c *Config) IsLoaded() bool {
	return c.PathName != "" && c.Sections != nil
}

//加载配置
func (c *Config) LoadConfig(path string) error {
	c.Sections = make(map[string]ConfigSection)
	c.PathName = path

	f, err := os.Open(path)
	defer f.Close()
	if err != nil {
		return err
	}

	r := bufio.NewReader(f)

	var cursec ConfigSection

	for {
		sbytes, _, err := r.ReadLine()

		if err != nil {
			if err == io.EOF {
				break
			}

			return err
		}

		//删除换行和空格
		sline := string(sbytes)
		//对于有"的字符行,不做空格处理
		if strings.Index(sline, "\"") == -1 {
			sline = util.TrimAllSpace(sline)
		}
		sline = util.TrimEnter(sline)

		//空白行
		if sline == "" {
			continue
		}

		//新节点
		if strings.Index(sline, "[") != -1 && strings.Index(sline, "]") != -1 {
			cursec = NewConfigSection()
			if _, ok := c.Sections[sline]; ok {
				panic(fmt.Sprintf("配置文件%s,节点%s重复", path, sline))
			}

			secname := strings.Replace(sline, "[", "", -1)
			secname = strings.Replace(secname, "]", "", -1)

			c.Sections[secname] = cursec

			continue
		}

		//检查当前配置节对象是否创建
		if cursec == nil {
			panic("配置数据必须以配置节开始")
		}

		//注释行
		if strings.Index(sline, "#") == 0 {
			continue
		}

		pos := strings.Index(sline, "=")
		if pos == -1 {
			continue
		}

		key := sline[:pos]
		value := sline[pos+1:]

		if strings.Index(value, "#") != -1 {
			endpos := strings.Index(value, "#")
			value = value[:endpos]
		}

		if strings.Index(value, "//") != -1 {
			endpos := strings.Index(value, "//")
			value = value[:endpos]
		}

		if value == "" || key == "" {
			continue
		}

		cursec[key] = value

	}

	return nil
}

//获取配置节
func (c *Config) GetConfigSection(name string) ConfigSection {
	cv, ok := c.Sections[name]

	if !ok {
		return nil
	}

	return cv
}

//获取配置值
func (c *Config) GetConfigValue(name string, key string) string {
	cs := c.GetConfigSection(name)

	if cs == nil {
		return ""
	}

	return cs[key]
}

//获取配置数字
func (c *Config) GetConfigInt(name string, key string) int {
	v := c.GetConfigValue(name, key)

	if v != "" {
		ret, _ := strconv.Atoi(v)

		return ret
	}

	return 0
}

//获取配置int32值
func (c *Config) GetConfigInt32(name string, key string) int32 {
	return int32(c.GetConfigInt(name, key))
}

//获取配置int64值
func (c *Config) GetConfigInt64(name string, key string) int64 {
	return int64(c.GetConfigInt(name, key))
}

//获取配置float32值
func (c *Config) GetConfigFloat32(name string, key string) float32 {
	return float32(c.GetConfigFloat64(name, key))
}

//获取配置float64值
func (c *Config) GetConfigFloat64(name string, key string) float64 {
	val := c.GetConfigValue(name, key)

	if val == "" {
		return 0
	}

	valf, err := strconv.ParseFloat(val, 32)

	if err != nil {
		return 0
	}

	return valf
}

//加载配置
func GetConfig(path string) *Config {
	cfg := new(Config)
	err := cfg.LoadConfig(path)

	if err != nil {
		return nil
	}

	return cfg
}

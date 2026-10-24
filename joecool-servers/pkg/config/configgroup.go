package config

import (
	"bytes"
	"fmt"
	"io/ioutil"
	"os"
	"path"
	"regexp"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/conf"

	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//配置组
type ConfigGroup struct {
	Configs map[string]*Config //配置数据
	DirName string             //目录名
}

//转换为字符串
func (cfg *ConfigGroup) String() string {
	var buf bytes.Buffer

	for k, v := range cfg.Configs {
		buf.WriteString("{" + k + "}\r\n")
		buf.WriteString(v.String())
		buf.WriteString("\r\n\r\n")
	}

	return buf.String()
}

//是否加载
func (cfg *ConfigGroup) IsLoaded() bool {
	return cfg.DirName != "" && cfg.Configs != nil
}

//加载配置
func (cfg *ConfigGroup) LoadConfig(dirname string, depth int) error {
	cfg.DirName = dirname
	if depth == 0 {
		cfg.Configs = make(map[string]*Config)
	}

	flist, err := ioutil.ReadDir(dirname)

	if err != nil {
		return err
	}

	for _, fi := range flist {
		if fi.IsDir() {
			cfg.LoadConfig(path.Join(dirname, fi.Name()), depth+1)
		} else {
			cf := GetConfig(path.Join(dirname, fi.Name()))
			if cf != nil {
				fname := fi.Name()
				if conf.TestMode {
					var re = regexp.MustCompile(`.example$`)
					fname = re.ReplaceAllString(fname, "")
				}
				cfName := getConfigName(path.Join(dirname, fname), depth)

				if cfName != "" {

					cfg.Configs[cfName] = cf
				}
			}
		}
	}

	return nil
}

//获取配置名称
func getConfigName(pathname string, depth int) string {
	sarr := strings.Split(pathname, "/")
	slen := len(sarr)

	if depth+1 > slen {
		panic(fmt.Sprintf("%s,depth:%d", pathname, depth))
	}

	cfgName := ""

	for depth >= 0 {
		cfgName += sarr[slen-depth-1]
		cfgName += "_"

		depth--
	}

	cfgName = strings.TrimRight(cfgName, "_")
	extname := path.Ext(pathname)

	cfgName = strings.Replace(cfgName, extname, "", 1)
	return cfgName
}

//获取配置组
func GetConfigGroup(dirname string) *ConfigGroup {
	pathname, err := util.GetCurrentPath()

	if err != nil {
		panic(err)
	}

	dirname = pathname + dirname
	//fmt.Println(dirname)

	cfg := new(ConfigGroup)
	cfg.LoadConfig(dirname, 0)

	//添加命令
	cmd := NewConfigCmd(cfg)
	command.AddCommand(cmd)

	return cfg
}

// LoadConfigGroup 获取配置组
func LoadConfigGroup(path string) (*ConfigGroup, error) {

	data := new(ConfigGroup)

	fi, err := os.Stat(path)
	if err != nil {
		return nil, fmt.Errorf("Find config folder %s Error", path)
	}

	if !fi.IsDir() {
		return nil, fmt.Errorf("%s is not a folder", path)
	}

	data.LoadConfig(path, 0)

	if data == nil {
		return nil, fmt.Errorf("Can not find config folder, please put setting files on this folder")
	}

	if len(data.Configs) == 0 {
		return nil, fmt.Errorf("Can not load configs, path: %s", data.DirName)
	}

	return data, nil
}

//获取配置
func (cfg *ConfigGroup) GetConfig(name string) *Config {
	if cf, ok := cfg.Configs[name]; ok {
		return cf
	}

	return nil
}

//获取配置节
func (cfg *ConfigGroup) GetConfigSection(name string, sname string) ConfigSection {
	cf := cfg.GetConfig(name)
	if cf == nil {
		return nil
	}

	return cf.GetConfigSection(sname)
}

//获取配置值
func (cfg *ConfigGroup) GetConfigValue(name string, sname string, keyname string) string {
	cfs := cfg.GetConfigSection(name, sname)

	if cfs == nil {
		return ""
	}

	return cfs[keyname]
}

package room

type Conf struct {
	Game     string `yaml:"game"`
	ServerID int32  `yaml:"serverid"`
	Addr     string `yaml:"addr"`
}

type ConfList []Conf

type ConfMap map[int32]Conf

package util

import (
	"net"
	"strconv"
	"strings"
)

//解析地址中的IP和端口
func GetIPAndPort(addr net.Addr) (string, int) {
	return ParseIPAndPort(addr.String())
}

//从字符串中解析IP和端口
func ParseIPAndPort(addr string) (string, int) {
	if addr == "" {
		return "unknown", -1
	}

	data := strings.Split(addr, ":")
	ip := data[0]
	sport := data[1]

	port, _ := strconv.Atoi(sport)

	return ip, port
}

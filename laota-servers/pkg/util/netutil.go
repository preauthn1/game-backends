package util

import (
	"net"
	"strconv"
	"strings"
	"fmt"
)

//
const (
	IP_IS_V4 = "ipv4"
	IP_IS_V6 = "ipv6"
	IP_UNKNOWN = "unknown"
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

//从字符串解析是否为 IPv4 或 IPv6, 正确 err 回传 nil
func ParseIPv4orIPv6(addr string) (ip net.IP, ipv4orv6 string, err error) {
	var validIP = net.ParseIP(addr)
	if validIP != nil {
		// check IPv4, To4只能对IPv4地址进行转换
		if validIP.To4() != nil {
			return validIP, IP_IS_V4, nil
		}

		//check IPv6, To16转换能对IPv4和IPv6
		if validIP.To16() != nil {
			return validIP, IP_IS_V6, nil
		}

	}
	return nil, IP_UNKNOWN, fmt.Errorf("not valid IP address. [%s]", addr)
}

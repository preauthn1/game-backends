package util

import (
	"net"
	"strings"
)

type NetworkFilter struct {
	networkList []string
}

func NewNetworkFilter(list string) *NetworkFilter {
	nf := new(NetworkFilter)
	nf.networkList = strings.Split(list, ",")
	return nf
}

func (nf *NetworkFilter) AddNetwork(ns string) {
	nf.networkList = append(nf.networkList, ns)
}

func (nf *NetworkFilter) CheckIPContains(addr string) bool {
	rAddr := net.ParseIP(addr)
	for _, n := range nf.networkList {
		_, ipNet, err := net.ParseCIDR(n)
		if err != nil {
			ipHost := net.ParseIP(n)
			if ipHost != nil {
				if ipHost.Equal(rAddr) {
					return true
				}
			}
		} else {
			if ipNet.Contains(rAddr) {
				return true
			}
		}
	}
	return false
}

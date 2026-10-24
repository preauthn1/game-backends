package util

import (
	"math/rand"
	"time"
)

//随机种子
var RndSeed rand.Source = rand.NewSource(time.Now().UnixNano())

//生成范围内的随机整数, 不包含max
func RndInt(min int, max int) int {
	r := rand.New(RndSeed)

	v := r.Intn(max - min)

	return min + v
}

//生成0,1范围内的随机小数
func RndFloat() float32 {
	r := rand.New(RndSeed)
	return r.Float32()
}

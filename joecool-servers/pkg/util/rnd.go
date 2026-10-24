package util

import (
	"math/rand"
	"time"
)

func init() {
	// 實際上使用 globalRand 來產生隨機數就行了
	// 自訂義的 var RndSeed rand.Source = rand.NewSource(time.Now().UnixNano()) 會有線程安全問題
	// NewSource returns a new pseudo-random Source seeded with the given value.
	// Unlike the default Source used by top-level functions, this source is not
	// safe for concurrent use by multiple goroutines.
	rand.Seed(time.Now().UnixNano())
}

//生成范围内的随机整数, 不包含max
func RndInt(min int, max int) int {
	v := rand.Intn(max - min)
	return min + v
}

//生成0,1范围内的随机小数
func RndFloat() float32 {
	return rand.Float32()
}

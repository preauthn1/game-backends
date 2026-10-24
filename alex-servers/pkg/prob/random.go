package prob

import (
	"math/rand"
)

// Random 可Add物件及機率，再以Choose隨機選出一個
type Random struct {
	objs  []interface{}
	total int   //總重量
	split []int // 分水嶺
}

// Add 加入物件及機率
func (r *Random) Add(i interface{}, weight int) {
	r.total += weight
	r.objs = append(r.objs, i)
	r.split = append(r.split, r.total)
}

// Choose 隨機選出一個
func (r *Random) Choose() interface{} {
	if r.total == 0 {
		return nil
	}

	// 隨機產生該總重量
	random := rand.Intn(r.total)

	// 找出是哪個
	for i, row := range r.split {
		if random < row {
			return r.objs[i]
		}
	}

	return nil
}

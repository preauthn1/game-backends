package prob

import (
	"math/rand"
	"strconv"
	"strings"
	"testing"
	"time"
)

func TestRandom(t *testing.T) {

	rand.Seed(int64(time.Now().Nanosecond()))

	str := "3:1000,5:200,8:50"

	multis := strings.Split(str, ",")

	r := new(Random)

	for _, row := range multis {
		vals := strings.Split(row, ":")

		m, _ := strconv.ParseFloat(vals[0], 64)
		w, _ := strconv.ParseFloat(vals[1], 64)

		r.Add(int64(m), int(w))
	}

	t.Log(r.Choose().(int64))

}

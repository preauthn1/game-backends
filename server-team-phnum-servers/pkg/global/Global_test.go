package global

import (
	"testing"
)

func TestMoneyString(t *testing.T) {
	ary_i64Money := []int64{
		int64(-2345),
		int64(-2305),
		int64(-100),
		int64(-99),
		int64(-1),
		int64(-0),
		int64(0),
		int64(1),
		int64(99),
		int64(100),
		int64(2305),
		int64(2345),
	}

	for i := 0; i < len(ary_i64Money); i++ {
		t.Logf("Money = %d, String = %s\n", ary_i64Money[i], MoneyString(ary_i64Money[i]))
	}
}

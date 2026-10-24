package roulette

import (
	"reflect"
	"testing"
)

func TestBet_Valid(t *testing.T) {

	// 單
	t.Run("單", func(t *testing.T) {
		b := Bet{
			Type:  Odd,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 雙
	t.Run("雙", func(t *testing.T) {
		b := Bet{
			Type:  Even,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 紅
	t.Run("紅", func(t *testing.T) {
		b := Bet{
			Type:  Red,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 黑
	t.Run("黑", func(t *testing.T) {
		b := Bet{
			Type:  Black,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 小
	t.Run("小", func(t *testing.T) {
		b := Bet{
			Type:  Small,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 大
	t.Run("大", func(t *testing.T) {
		b := Bet{
			Type:  Big,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 1ST12
	t.Run("1ST12", func(t *testing.T) {
		b := Bet{
			Type:  First12,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 2ND12
	t.Run("2ND12", func(t *testing.T) {
		b := Bet{
			Type:  Second12,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 3RD12
	t.Run("3RD12", func(t *testing.T) {
		b := Bet{
			Type:  Third12,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 1ST ROW
	t.Run("1ST ROW", func(t *testing.T) {
		b := Bet{
			Type:  FirstRow,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 2ND ROW
	t.Run("2ND ROW", func(t *testing.T) {
		b := Bet{
			Type:  SecondRow,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 3RD ROW
	t.Run("3RD ROW", func(t *testing.T) {
		b := Bet{
			Type:  ThirdRow,
			Count: 1,
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	// 1個號碼
	t.Run("1個號碼", func(t *testing.T) {
		b := Bet{
			Type:  OneNumber,
			Count: 1,
			Numbers: []uint32{
				1,
			},
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		for i := uint32(2); i <= 36; i++ {
			b.Numbers = []uint32{i}
			if err := b.Valid(); err != nil {
				t.Error(err)
			}
		}

		b.Numbers = []uint32{0}
		if err := b.Valid(); err == nil {
			t.Error("0不是有效的號碼")
		}
	})
	// 2個號碼
	t.Run("2個號碼", func(t *testing.T) {
		b := Bet{
			Type:    TwoNumber,
			Count:   1,
			Numbers: []uint32{0, 1},
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{0, 2}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{0, 3}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{1, 2}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{2, 3}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{4, 5}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{5, 6}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{7, 8}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{8, 9}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{10, 11}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{11, 12}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{13, 14}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{14, 15}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{16, 17}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{17, 18}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{19, 20}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{20, 21}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{22, 23}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{23, 24}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{25, 26}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{26, 27}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{28, 29}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{29, 30}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{31, 32}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{32, 33}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{34, 35}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{35, 36}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{1, 4}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{2, 5}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{3, 6}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{4, 7}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{5, 8}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{6, 9}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{7, 10}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{8, 11}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{9, 12}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{10, 13}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{11, 14}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{12, 15}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{13, 16}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{14, 17}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{15, 18}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{16, 19}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{17, 20}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{18, 21}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{19, 22}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{20, 23}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{21, 24}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{22, 25}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{23, 26}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{24, 27}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{25, 28}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{26, 29}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{27, 30}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{28, 31}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{29, 32}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{30, 33}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{31, 34}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{32, 35}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{33, 36}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	t.Run("2個號碼", func(t *testing.T) {
		b := Bet{
			Type:  TwoNumber,
			Count: 1,
			Numbers: []uint32{
				1, 3,
			},
		}
		t.Log(b.Valid())
		if err := b.Valid(); reflect.TypeOf(err) != reflect.TypeOf(&ErrorIllegalBetNumber{}) {
			t.Error(err)
		}
	})
	// 3個號碼
	t.Run("3個號碼", func(t *testing.T) {
		b := Bet{
			Type:  ThreeNumber,
			Count: 1,
			Numbers: []uint32{
				1, 2, 3,
			},
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{4, 5, 6}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{7, 8, 9}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{10, 11, 12}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{13, 14, 15}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{16, 17, 18}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{19, 20, 21}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{22, 23, 24}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{25, 26, 27}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{28, 29, 30}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{31, 32, 33}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{34, 35, 36}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	t.Run("3個號碼", func(t *testing.T) {
		b := Bet{
			Type:  ThreeNumber,
			Count: 1,
			Numbers: []uint32{
				2, 3, 4,
			},
		}
		if err := b.Valid(); reflect.TypeOf(err) != reflect.TypeOf(&ErrorIllegalBetNumber{}) {
			t.Error(err)
		}
	})
	// 4個號碼
	t.Run("4個號碼", func(t *testing.T) {
		b := Bet{
			Type:  FourNumber,
			Count: 1,
			Numbers: []uint32{
				1, 2, 4, 5,
			},
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{2, 3, 5, 6}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{4, 5, 7, 8}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{5, 6, 8, 9}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{7, 8, 10, 11}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{8, 9, 11, 12}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{10, 11, 13, 14}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{11, 12, 14, 15}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{13, 14, 16, 17}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{14, 15, 17, 18}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{16, 17, 19, 20}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{17, 18, 20, 21}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{19, 20, 22, 23}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{20, 21, 23, 24}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{22, 23, 25, 26}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{23, 24, 26, 27}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{25, 26, 28, 29}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{26, 27, 29, 30}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{28, 29, 31, 32}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{29, 30, 32, 33}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{31, 32, 34, 35}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{32, 33, 35, 36}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	t.Run("4個號碼", func(t *testing.T) {
		b := Bet{
			Type:  FourNumber,
			Count: 1,
			Numbers: []uint32{
				1, 4, 7, 10,
			},
		}
		if err := b.Valid(); reflect.TypeOf(err) != reflect.TypeOf(&ErrorIllegalBetNumber{}) {
			t.Error(err)
		}

		b.Numbers = []uint32{2, 5, 8, 11}
		if err := b.Valid(); reflect.TypeOf(err) != reflect.TypeOf(&ErrorIllegalBetNumber{}) {
			t.Error(err)
		}

		b.Numbers = []uint32{1, 2, 3, 4}
		if err := b.Valid(); reflect.TypeOf(err) != reflect.TypeOf(&ErrorIllegalBetNumber{}) {
			t.Error(err)
		}
	})
	// 6個號碼
	t.Run("6個號碼", func(t *testing.T) {
		b := Bet{
			Type:  SixNumber,
			Count: 1,
			Numbers: []uint32{
				1, 2, 3, 4, 5, 6,
			},
		}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{4, 5, 6, 7, 8, 9}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{7, 8, 9, 10, 11, 12}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{10, 11, 12, 13, 14, 15}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{13, 14, 15, 16, 17, 18}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{16, 17, 18, 19, 20, 21}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{19, 20, 21, 22, 23, 24}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{22, 23, 24, 25, 26, 27}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{25, 26, 27, 28, 29, 30}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{28, 29, 30, 31, 32, 33}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}

		b.Numbers = []uint32{31, 32, 33, 34, 35, 36}
		if err := b.Valid(); err != nil {
			t.Error(err)
		}
	})
	t.Run("6個號碼", func(t *testing.T) {
		b := Bet{
			Type:  SixNumber,
			Count: 1,
			Numbers: []uint32{
				1, 4, 7, 10, 13, 16,
			},
		}
		if err := b.Valid(); reflect.TypeOf(err) != reflect.TypeOf(&ErrorIllegalBetNumber{}) {
			t.Error(err)
		}

		b.Numbers = []uint32{2, 5, 8, 11, 14, 17}
		if err := b.Valid(); reflect.TypeOf(err) != reflect.TypeOf(&ErrorIllegalBetNumber{}) {
			t.Error(err)
		}

	})
}

func TestBet_Match(t *testing.T) {

	// 紅
	t.Run("紅", func(t *testing.T) {
		reds := []uint32{1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36}
		blacks := []uint32{2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35}
		b := Bet{
			Type:  Red,
			Count: 1,
		}
		for _, v := range reds {
			if !b.Match(v) {
				t.Errorf("%d 應該中獎", v)
			}
		}
		for _, v := range blacks {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}
		if b.Match(0) {
			t.Error("0 不應該中獎")
		}
	})

	// 黑
	t.Run("黑", func(t *testing.T) {
		reds := []uint32{1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36}
		blacks := []uint32{2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35}
		b := Bet{
			Type:  Black,
			Count: 1,
		}
		for _, v := range reds {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}
		for _, v := range blacks {
			if !b.Match(v) {
				t.Errorf("%d 應該中獎", v)
			}
		}
		if b.Match(0) {
			t.Error("0 不應該中獎")
		}
	})

	// 單
	t.Run("單", func(t *testing.T) {
		odds := []uint32{1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35}
		evens := []uint32{2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36}
		b := Bet{
			Type:  Odd,
			Count: 1,
		}
		for _, v := range odds {
			if !b.Match(v) {
				t.Errorf("%d 應該中獎", v)
			}
		}
		for _, v := range evens {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}
		if b.Match(0) {
			t.Error("0 不應該中獎")
		}
	})

	// 雙
	t.Run("雙", func(t *testing.T) {
		odds := []uint32{1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35}
		evens := []uint32{2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36}
		b := Bet{
			Type:  Even,
			Count: 1,
		}
		for _, v := range odds {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}
		for _, v := range evens {
			if !b.Match(v) {
				t.Errorf("%d 應該中獎", v)
			}
		}
		if b.Match(0) {
			t.Error("0 不應該中獎")
		}
	})

	// 大
	t.Run("大", func(t *testing.T) {
		b := Bet{
			Type:  Big,
			Count: 1,
		}
		for i := 1; i <= 18; i++ {
			if b.Match(uint32(i)) {
				t.Errorf("%d 不應該中獎", i)
			}
		}
		for i := 19; i <= 36; i++ {
			if !b.Match(uint32(i)) {
				t.Errorf("%d 應該中獎", i)
			}
		}
		if b.Match(0) {
			t.Error("0 不應該中獎")
		}
	})

	// 小
	t.Run("小", func(t *testing.T) {
		b := Bet{
			Type:  Small,
			Count: 1,
		}
		for i := 1; i <= 18; i++ {
			if !b.Match(uint32(i)) {
				t.Errorf("%d 應該中獎", i)
			}
		}
		for i := 19; i <= 36; i++ {
			if b.Match(uint32(i)) {
				t.Errorf("%d 不應該中獎", i)
			}
		}
		if b.Match(0) {
			t.Error("0 不應該中獎")
		}
	})

	// 1ST 12
	t.Run("1ST 12", func(t *testing.T) {
		b := Bet{
			Type:  First12,
			Count: 1,
		}
		for i := 1; i <= 12; i++ {
			if !b.Match(uint32(i)) {
				t.Errorf("%d 應該中獎", i)
			}
		}
		for i := 13; i <= 36; i++ {
			if b.Match(uint32(i)) {
				t.Errorf("%d 不應該中獎", i)
			}
		}
		if b.Match(0) {
			t.Error("0 不應該中獎")
		}
	})

	// 2ND 12
	t.Run("2ND 12", func(t *testing.T) {
		b := Bet{
			Type:  Second12,
			Count: 1,
		}
		for i := 1; i <= 12; i++ {
			if b.Match(uint32(i)) {
				t.Errorf("%d 不應該中獎", i)
			}
		}
		for i := 13; i <= 24; i++ {
			if !b.Match(uint32(i)) {
				t.Errorf("%d 應該中獎", i)
			}
		}
		for i := 25; i <= 36; i++ {
			if b.Match(uint32(i)) {
				t.Errorf("%d 不應該中獎", i)
			}
		}
		if b.Match(0) {
			t.Error("0 不應該中獎")
		}
	})

	// 3RD 12
	t.Run("3RD 12", func(t *testing.T) {
		b := Bet{
			Type:  Third12,
			Count: 1,
		}
		for i := 1; i <= 24; i++ {
			if b.Match(uint32(i)) {
				t.Errorf("%d 不應該中獎", i)
			}
		}
		for i := 25; i <= 36; i++ {
			if !b.Match(uint32(i)) {
				t.Errorf("%d 應該中獎", i)
			}
		}
		if b.Match(0) {
			t.Error("0 不應該中獎")
		}
	})

	// 1ST ROW
	t.Run("1ST ROW", func(t *testing.T) {
		b := Bet{
			Type:  FirstRow,
			Count: 1,
		}

		row1 := []uint32{1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34}
		row2 := []uint32{2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35}
		row3 := []uint32{3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36}

		for _, v := range row1 {
			if !b.Match(v) {
				t.Errorf("%d 應該中獎", v)
			}
		}

		for _, v := range row2 {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}

		for _, v := range row3 {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}

		if b.Match(0) {
			t.Error("0 不應該中獎")
		}

	})

	// 2ND ROW
	t.Run("2ND ROW", func(t *testing.T) {
		b := Bet{
			Type:  SecondRow,
			Count: 1,
		}

		row1 := []uint32{1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34}
		row2 := []uint32{2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35}
		row3 := []uint32{3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36}

		for _, v := range row1 {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}

		for _, v := range row2 {
			if !b.Match(v) {
				t.Errorf("%d 應該中獎", v)
			}
		}

		for _, v := range row3 {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}

		if b.Match(0) {
			t.Error("0 不應該中獎")
		}

	})

	// 3RD ROW
	t.Run("3RD ROW", func(t *testing.T) {
		b := Bet{
			Type:  ThirdRow,
			Count: 1,
		}

		row1 := []uint32{1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34}
		row2 := []uint32{2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35}
		row3 := []uint32{3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36}

		for _, v := range row1 {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}

		for _, v := range row2 {
			if b.Match(v) {
				t.Errorf("%d 不應該中獎", v)
			}
		}

		for _, v := range row3 {
			if !b.Match(v) {
				t.Errorf("%d 應該中獎", v)
			}
		}

		if b.Match(0) {
			t.Error("0 不應該中獎")
		}

	})

	// One Number
	t.Run("One Number", func(t *testing.T) {
		b := Bet{
			Type:    OneNumber,
			Count:   1,
			Numbers: []uint32{5},
		}

		for i := 0; i <= 36; i++ {
			if i == 5 {
				if !b.Match(uint32(i)) {
					t.Errorf("%d 應該中獎", i)
				}
			} else {
				if b.Match(uint32(i)) {
					t.Errorf("%d 不應該中獎", i)
				}
			}
		}
	})

	// Two Number
	t.Run("Two Number", func(t *testing.T) {
		b := Bet{
			Type:    TwoNumber,
			Count:   1,
			Numbers: []uint32{5, 6},
		}

		for i := 0; i <= 36; i++ {
			if i == 5 || i == 6 {
				if !b.Match(uint32(i)) {
					t.Errorf("%d 應該中獎", i)
				}
			} else {
				if b.Match(uint32(i)) {
					t.Errorf("%d 不應該中獎", i)
				}
			}
		}
	})

	// Three Number
	t.Run("Three Number", func(t *testing.T) {
		b := Bet{
			Type:    ThreeNumber,
			Count:   1,
			Numbers: []uint32{19, 20, 21},
		}

		for i := 0; i <= 36; i++ {
			if i == 19 || i == 20 || i == 21 {
				if !b.Match(uint32(i)) {
					t.Errorf("%d 應該中獎", i)
				}
			} else {
				if b.Match(uint32(i)) {
					t.Errorf("%d 不應該中獎", i)
				}
			}
		}
	})

	// Four Number
	t.Run("Four Number", func(t *testing.T) {
		b := Bet{
			Type:    FourNumber,
			Count:   1,
			Numbers: []uint32{19, 20, 22, 23},
		}

		for i := 0; i <= 36; i++ {
			if i == 19 || i == 20 || i == 22 || i == 23 {
				if !b.Match(uint32(i)) {
					t.Errorf("%d 應該中獎", i)
				}
			} else {
				if b.Match(uint32(i)) {
					t.Errorf("%d 不應該中獎", i)
				}
			}
		}
	})

	// Six Number
	t.Run("Six Number", func(t *testing.T) {
		b := Bet{
			Type:    SixNumber,
			Count:   1,
			Numbers: []uint32{19, 20, 21, 22, 23, 24},
		}

		for i := 0; i <= 36; i++ {
			if i == 19 || i == 20 || i == 21 || i == 22 || i == 23 || i == 24 {
				if !b.Match(uint32(i)) {
					t.Errorf("%d 應該中獎", i)
				}
			} else {
				if b.Match(uint32(i)) {
					t.Errorf("%d 不應該中獎", i)
				}
			}
		}
	})
}

func TestBet_Payout(t *testing.T) {

	conf := NewBetConfig(DefaultBetTypes...)

	t.Run("Dry Run", func(t *testing.T) {
		b := Bet{
			Type:  Red,
			Count: 1,
		}

		rtn, win := b.Payout(*conf, true)
		if rtn != 1 {
			t.Errorf("rtn 應該回傳 1")
		}
		if win != 1 {
			t.Errorf("win 應該回傳 1")
		}

		if b.Win != 0 {
			t.Errorf("b.Win 應該為 0")
		}
	})

	t.Run("Real Run", func(t *testing.T) {
		b := Bet{
			Type:  Red,
			Count: 1,
		}

		rtn, win := b.Payout(*conf, false)
		if rtn != 1 {
			t.Errorf("rtn 應該回傳 1")
		}
		if win != 1 {
			t.Errorf("win 應該回傳 1")
		}

		if b.Win != 1 {
			t.Errorf("b.Win 應該為 1")
		}
	})

}

func TestBetConfig_Allow(t *testing.T) {
	conf := NewBetConfig(DefaultBetTypes...)

	conf.AddAllowAmounts(5, 10, 50, 100)

	if !conf.Allow(500) {
		t.Error("500 應該可以下注")
	}

	if !conf.Allow(350) {
		t.Error("350 應該可以下注")
	}

	if !conf.Allow(225) {
		t.Error("225 應該可以下注")
	}

	if !conf.Allow(125) {
		t.Error("125 應該可以下注")
	}

	if conf.Allow(77) {
		t.Error("77 不應該可以下注")
	}

}

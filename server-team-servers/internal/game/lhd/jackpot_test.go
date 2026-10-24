package lhd

import "testing"

func TestJackpot_AddBet(t *testing.T) {

	jp := NewJackpot()
	jp.Enabled = true

	t.Run("Bet 100", func(t *testing.T) {
		contribute := jp.AddBet(100)
		if jp.Contribute != 20000 {
			t.Errorf("jp.Contribute != 20000, jp.Contribute = %d", jp.Contribute)
		}
		if contribute != 20000 {
			t.Errorf("contribute != 20000, contribute = %d", contribute)
		}
	})

	t.Run("Bet 1000", func(t *testing.T) {
		contribute := jp.AddBet(1000)
		if jp.Contribute != 220000 {
			t.Errorf("jp.Contribute != 220000, jp.Contribute = %d", jp.Contribute)
		}
		if contribute != 200000 {
			t.Errorf("contribute != 200000, contribute = %d", contribute)
		}
	})

	if phase, _ := jp.Phase(); phase != 1 {
		t.Errorf("jp.Phase() != 1, jp.Phase() = %d", phase)
	}

	t.Run("Bet 10000000", func(t *testing.T) {
		contribute := jp.AddBet(10000000)
		if jp.Contribute != 2000220000 {
			t.Errorf("jp.Contribute != 2000220000, jp.Contribute = %d", jp.Contribute)
		}
		if contribute != 2000000000 {
			t.Errorf("contribute != 2000000000, contribute = %d", contribute)
		}
	})

	if jp.Current() != 700022 {
		t.Errorf("jp.Current() != 700022, j.Current() = %d", jp.Current())
	}

	if phase, _ := jp.Phase(); phase != 3 {
		t.Errorf("jp.Phase() != 3, jp.Phase() = %d", phase)
	}

	t.Run("Bet 10000000", func(t *testing.T) {
		contribute := jp.AddBet(10000000)
		if jp.Contribute != 4000220000 {
			t.Errorf("jp.Contribute != 4000220000, jp.Contribute = %d", jp.Contribute)
		}
		if contribute != 2000000000 {
			t.Errorf("contribute != 2000000000, contribute = %d", contribute)
		}
	})

	if phase, _ := jp.Phase(); phase != 4 {
		t.Errorf("jp.Phase() != 4, jp.Phase() = %d", phase)
	}
}

func TestJackpot_Award(t *testing.T) {

	t.Run("Award Mini", func(t *testing.T) {
		jp := NewJackpot()
		jp.Enabled = true
		jp.Contribute = 5000000000

		if jp.Current() != 1000000 {
			t.Errorf("jp.Current() != 1000000, j.Current() = %d", jp.Current())
		}

		jp.AwardWeight[JackpotLevelNone] = 0
		jp.AwardWeight[JackpotLevelMini] = 1
		jp.AwardWeight[JackpotLevelMinor] = 0
		jp.AwardWeight[JackpotLevelGrand] = 0

		lv, amt, contribute := jp.Award(10000, 10000)
		if lv != JackpotLevelMini {
			t.Errorf("lv != JackpotLevelMini, lv = %d", lv)
		}
		if amt != 100000 {
			t.Errorf("amt != 100000, amt = %d", amt)
		}
		if contribute != -1000000000 {
			t.Errorf("contribute != -1000000000, contribute = %d", contribute)
		}

		if jp.Current() != 900000 {
			t.Errorf("jp.Current() != 900000, j.Current() = %d", jp.Current())
		}
	})

	t.Run("Award Minor", func(t *testing.T) {
		jp := NewJackpot()
		jp.Enabled = true
		jp.Contribute = 5000000000

		if jp.Current() != 1000000 {
			t.Errorf("jp.Current() != 1000000, j.Current() = %d", jp.Current())
		}

		jp.AwardWeight[JackpotLevelNone] = 0
		jp.AwardWeight[JackpotLevelMini] = 0
		jp.AwardWeight[JackpotLevelMinor] = 1
		jp.AwardWeight[JackpotLevelGrand] = 0

		lv, amt, contribute := jp.Award(10000, 10000)
		if lv != JackpotLevelMinor {
			t.Errorf("lv != JackpotLevelMinor, lv = %d", lv)
		}
		if amt != 200000 {
			t.Errorf("amt != 200000, amt = %d", amt)
		}
		if contribute != -2000000000 {
			t.Errorf("contribute != -2000000000, contribute = %d", contribute)
		}

		if jp.Current() != 800000 {
			t.Errorf("jp.Current() != 800000, j.Current() = %d", jp.Current())
		}
	})

	t.Run("Award Grand", func(t *testing.T) {
		jp := NewJackpot()
		jp.Enabled = true
		jp.Contribute = 5000000000

		if jp.Current() != 1000000 {
			t.Errorf("jp.Current() != 1000000, j.Current() = %d", jp.Current())
		}

		jp.AwardWeight[JackpotLevelNone] = 0
		jp.AwardWeight[JackpotLevelMini] = 0
		jp.AwardWeight[JackpotLevelMinor] = 0
		jp.AwardWeight[JackpotLevelGrand] = 1

		lv, amt, contribute := jp.Award(10000, 10000)
		if lv != JackpotLevelGrand {
			t.Errorf("lv != JackpotLevelGrand, lv = %d", lv)
		}
		if amt != 1000000 {
			t.Errorf("amt != 1000000, amt = %d", amt)
		}
		if contribute != -10000000000 {
			t.Errorf("contribute != -10000000000, contribute = %d", contribute)
		}

		if jp.Current() != 500000 {
			t.Errorf("jp.Current() != 500000, j.Current() = %d", jp.Current())
		}
	})

}

func Test_Award(t *testing.T) {

	if testing.Short() {
		t.Skip("skipping test in short mode.")
		return
	}

	t.Run("Bet 19", func(t *testing.T) {
		jp := NewJackpot()
		jp.Enabled = true

		jp.AwardWeight[JackpotLevelNone] = 1
		jp.AwardWeight[JackpotLevelMini] = 1
		jp.AwardWeight[JackpotLevelMinor] = 1
		jp.AwardWeight[JackpotLevelGrand] = 1

		awardMap := make(map[JackpotLevel]int)

		for i := 0; i < 100000; i++ {
			jp.Contribute = 5000000000
			jplv, _, _ := jp.Award(1900, 30000)
			awardMap[jplv]++
		}

		for lv, cnt := range awardMap {
			t.Logf("lv: %d, cnt: %d", lv, cnt)
		}
	})

	t.Run("Bet 99", func(t *testing.T) {
		jp := NewJackpot()
		jp.Enabled = true

		jp.AwardWeight[JackpotLevelNone] = 1
		jp.AwardWeight[JackpotLevelMini] = 1
		jp.AwardWeight[JackpotLevelMinor] = 1
		jp.AwardWeight[JackpotLevelGrand] = 1

		awardMap := make(map[JackpotLevel]int)

		for i := 0; i < 100000; i++ {
			jp.Contribute = 5000000000
			jplv, _, _ := jp.Award(9900, 30000)
			awardMap[jplv]++
		}

		for lv, cnt := range awardMap {
			t.Logf("lv: %d, cnt: %d", lv, cnt)
		}
	})

	t.Run("Bet 101", func(t *testing.T) {
		jp := NewJackpot()
		jp.Enabled = true

		jp.AwardWeight[JackpotLevelNone] = 1
		jp.AwardWeight[JackpotLevelMini] = 1
		jp.AwardWeight[JackpotLevelMinor] = 1
		jp.AwardWeight[JackpotLevelGrand] = 1

		awardMap := make(map[JackpotLevel]int)

		for i := 0; i < 100000; i++ {
			jp.Contribute = 5000000000
			jplv, _, _ := jp.Award(10100, 30000)
			awardMap[jplv]++
		}

		for lv, cnt := range awardMap {
			t.Logf("lv: %d, cnt: %d", lv, cnt)
		}
	})

}

func TestJackpot_Phase(t *testing.T) {

	jp := NewJackpot()
	jp.Enabled = true

	t.Log(jp.PhaseLevel)

	t.Log(jp.Current())

	t.Log(jp.Phase())

	for i := 0; i < 20; i++ {
		jp.Contribute += 500000000
		t.Log(jp.Current())
		t.Log(jp.Phase())
	}

	// if current is 1482370.00
	jp.Contribute = 14823700000 - 5000000000
	t.Log(jp.Current())
	t.Log(jp.Phase())

}

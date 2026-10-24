package service

import "testing"

func TestBonusGame_Read(t *testing.T) {

	s := new(BonusGameService)

	t.Run("Test User", func(tt *testing.T) {
		data, err := s.Read(1, 1, 1)
		tt.Log(err)
		tt.Logf("Length: %d\n", len(data))
		for _, row := range data {
			tt.Log(row)
		}
	})

}

func TestBonusGame_Record(t *testing.T) {

	s := new(BonusGameService)

	t.Run("Test User", func(tt *testing.T) {
		tt.Log(s.Record(1, 1, 1, 1, 1, 0, 1, 1, ""))
		tt.Log(s.Record(1, 1, 1, 1, 2, 0, 1, 1, ""))
		tt.Log(s.Record(1, 1, 1, 1, 3, 0, 1, 1, ""))
		tt.Log(s.Record(1, 1, 1, 1, 4, 0, 1, 1, ""))
		tt.Log(s.Record(1, 1, 1, 1, 5, 0, 1, 1, ""))
	})

}

func TestBonusGame_Reset(t *testing.T) {

	s := new(BonusGameService)

	t.Run("Test User", func(tt *testing.T) {
		tt.Log(s.Reset(1, 1, 1, 0))
	})

}

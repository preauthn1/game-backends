package service

import "testing"

func TestGameLog_Add(t *testing.T) {

	s := new(GameLogService)

	t.Run("No User", func(tt *testing.T) {
		err := s.Add(1, 1, 123, 1001, 398, 100, 10, 30, 120, "p5,p5,p7|p3,p1,p7|p5,p1,p6|p2,p1,p7|p6,p7,p6")
		tt.Log(err)
	})

	t.Run("NewLog", func(tt *testing.T) {
		err := s.Add(1, 1, 9936738, 1001, 398, 100, 10, 30, 120, "p5,p5,p7|p3,p1,p7|p5,p1,p6|p2,p1,p7|p6,p7,p6")
		tt.Log(err)
	})

}

func TestGameLog_Search(t *testing.T) {

	s := new(GameLogService)

	t.Run("No User", func(tt *testing.T) {
		pageCnt, logs, err := s.Search(1, 1001, 1, 10, "2019-10-04", "2019-10-04")
		tt.Log(err)
		tt.Logf("All Row Count: %d\n", pageCnt)
		tt.Logf("Logs Length: %d\n", len(logs))
		for _, row := range logs {
			tt.Logf(" - %v\n", row)
		}
	})

	t.Run("No Game", func(tt *testing.T) {
		pageCnt, logs, err := s.Search(9936738, 999, 1, 10, "2019-10-04", "2019-10-04")
		tt.Log(err)
		tt.Logf("All Row Count: %d\n", pageCnt)
		tt.Logf("Logs Length: %d\n", len(logs))
		for _, row := range logs {
			tt.Logf(" - %v\n", row)
		}
	})

	t.Run("Pass", func(tt *testing.T) {
		pageCnt, logs, err := s.Search(18934817, 1002, 1, 10, "2019-09-01", "2019-10-04")
		tt.Log(err)
		tt.Logf("All Row Count: %d\n", pageCnt)
		tt.Logf("Logs Length: %d\n", len(logs))
		for _, row := range logs {
			tt.Logf(" - %v\n", row)
		}
	})

}

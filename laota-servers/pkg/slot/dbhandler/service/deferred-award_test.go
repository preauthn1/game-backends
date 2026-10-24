package service

import "testing"

func TestDeferredAwardService_Read(t *testing.T) {

	s := new(DeferredAwardService)

	t.Run("Test User", func(tt *testing.T) {
		data, err := s.Read(1, 1, 1, "Test")
		tt.Log(err)
		tt.Logf("Length: %d\n", len(data))
		for _, row := range data {
			tt.Log(row)
		}
	})

}

func TestDeferredAwardService_Record(t *testing.T) {

	s := new(DeferredAwardService)

	t.Run("Test User", func(tt *testing.T) {
		tt.Log(s.Record(1, 1, 1, "Test", 1, "qq"))
	})

}

func TestDeferredAwardService_Pop(t *testing.T) {

	s := new(DeferredAwardService)

	t.Run("Test User", func(tt *testing.T) {
		tt.Log(s.Pop(1, 1, 1, "Test"))
	})

}

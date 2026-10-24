package sml

import (
	"testing"
)

func TestBetType_Match(t *testing.T) {

	t.Run("Award Goal Left", func(t *testing.T) {
		award := GoalLeft
		if !Goal.Match(award) {
			t.Errorf("Goal Match Goal Left want %v, but %v", true, false)
		}
		if !GoalLeft.Match(award) {
			t.Errorf("GoalLeft Match Goal Left want %v, but %v", true, false)
		}
		if GoalMiddle.Match(award) {
			t.Errorf("GoalMiddle Match Goal Left want %v, but %v", false, true)
		}
		if GoalRight.Match(award) {
			t.Errorf("GoalRight Match Goal Left want %v, but %v", false, true)
		}

		if Save.Match(award) {
			t.Errorf("Save Match Goal Left want %v, but %v", false, true)
		}
		if SaveLeft.Match(award) {
			t.Errorf("SaveLeft Match Goal Left want %v, but %v", false, true)
		}
		if SaveMiddle.Match(award) {
			t.Errorf("SaveMiddle Match Goal Left want %v, but %v", false, true)
		}
		if SaveRight.Match(award) {
			t.Errorf("SaveRight Match Goal Left want %v, but %v", false, true)
		}
	})

	t.Run("Award Goal Middle", func(t *testing.T) {
		award := GoalMiddle
		if !Goal.Match(award) {
			t.Errorf("Goal Match Goal Middle want %v, but %v", true, false)
		}
		if GoalLeft.Match(award) {
			t.Errorf("GoalLeft Match Goal Middle want %v, but %v", false, true)
		}
		if !GoalMiddle.Match(award) {
			t.Errorf("GoalMiddle Match Goal Middle want %v, but %v", true, false)
		}
		if GoalRight.Match(award) {
			t.Errorf("GoalRight Match Goal Middle want %v, but %v", false, true)
		}

		if Save.Match(award) {
			t.Errorf("Save Match Goal Middle want %v, but %v", false, true)
		}
		if SaveLeft.Match(award) {
			t.Errorf("SaveLeft Match Goal Middle want %v, but %v", false, true)
		}
		if SaveMiddle.Match(award) {
			t.Errorf("SaveMiddle Match Goal Middle want %v, but %v", false, true)
		}
		if SaveRight.Match(award) {
			t.Errorf("SaveRight Match Goal Middle want %v, but %v", false, true)
		}
	})

	t.Run("Award Goal Right", func(t *testing.T) {
		award := GoalRight
		if !Goal.Match(award) {
			t.Errorf("Goal Match Goal Right want %v, but %v", true, false)
		}
		if GoalLeft.Match(award) {
			t.Errorf("GoalLeft Match Goal Right want %v, but %v", false, true)
		}
		if GoalMiddle.Match(award) {
			t.Errorf("GoalMiddle Match Goal Right want %v, but %v", false, true)
		}
		if !GoalRight.Match(award) {
			t.Errorf("GoalRight Match Goal Right want %v, but %v", true, false)
		}

		if Save.Match(award) {
			t.Errorf("Save Match Goal Right want %v, but %v", false, true)
		}
		if SaveLeft.Match(award) {
			t.Errorf("SaveLeft Match Goal Right want %v, but %v", false, true)
		}
		if SaveMiddle.Match(award) {
			t.Errorf("SaveMiddle Match Goal Right want %v, but %v", false, true)
		}
		if SaveRight.Match(award) {
			t.Errorf("SaveRight Match Goal Right want %v, but %v", false, true)
		}
	})

	t.Run("Award Save Left", func(t *testing.T) {
		award := SaveLeft
		if Goal.Match(award) {
			t.Errorf("Goal Match Save Left want %v, but %v", false, true)
		}
		if GoalLeft.Match(award) {
			t.Errorf("GoalLeft Match Save Left want %v, but %v", false, true)
		}
		if GoalMiddle.Match(award) {
			t.Errorf("GoalMiddle Match Save Left want %v, but %v", false, true)
		}
		if GoalRight.Match(award) {
			t.Errorf("GoalRight Match Save Left want %v, but %v", false, true)
		}

		if !Save.Match(award) {
			t.Errorf("Save Match Save Left want %v, but %v", true, false)
		}
		if !SaveLeft.Match(award) {
			t.Errorf("SaveLeft Match Save Left want %v, but %v", true, false)
		}
		if SaveMiddle.Match(award) {
			t.Errorf("SaveMiddle Match Save Left want %v, but %v", false, true)
		}
		if SaveRight.Match(award) {
			t.Errorf("SaveRight Match Save Left want %v, but %v", false, true)
		}
	})

	t.Run("Award Save Middle", func(t *testing.T) {
		award := SaveMiddle
		if Goal.Match(award) {
			t.Errorf("Goal Match Save Middle want %v, but %v", false, true)
		}
		if GoalLeft.Match(award) {
			t.Errorf("GoalLeft Match Save Middle want %v, but %v", false, true)
		}
		if GoalMiddle.Match(award) {
			t.Errorf("GoalMiddle Match Save Middle want %v, but %v", false, true)
		}
		if GoalRight.Match(award) {
			t.Errorf("GoalRight Match Save Middle want %v, but %v", false, true)
		}

		if !Save.Match(award) {
			t.Errorf("Save Match Save Middle want %v, but %v", true, false)
		}
		if SaveLeft.Match(award) {
			t.Errorf("SaveLeft Match Save Middle want %v, but %v", false, true)
		}
		if !SaveMiddle.Match(award) {
			t.Errorf("SaveMiddle Match Save Middle want %v, but %v", true, false)
		}
		if SaveRight.Match(award) {
			t.Errorf("SaveRight Match Save Middle want %v, but %v", false, true)
		}
	})

	t.Run("Award Goal Right", func(t *testing.T) {
		award := SaveRight
		if Goal.Match(award) {
			t.Errorf("Goal Match Save Right want %v, but %v", false, true)
		}
		if GoalLeft.Match(award) {
			t.Errorf("GoalLeft Match Save Right want %v, but %v", false, true)
		}
		if GoalMiddle.Match(award) {
			t.Errorf("GoalMiddle Match Save Right want %v, but %v", false, true)
		}
		if GoalRight.Match(award) {
			t.Errorf("GoalRight Match Save Right want %v, but %v", false, true)
		}

		if !Save.Match(award) {
			t.Errorf("Save Match Save Right want %v, but %v", true, false)
		}
		if SaveLeft.Match(award) {
			t.Errorf("SaveLeft Match Save Right want %v, but %v", false, true)
		}
		if SaveMiddle.Match(award) {
			t.Errorf("SaveMiddle Match Save Right want %v, but %v", false, true)
		}
		if !SaveRight.Match(award) {
			t.Errorf("SaveRight Match Save Right want %v, but %v", true, false)
		}
	})

}

func TestBetType_String(t *testing.T) {
	for _, tp := range BetAreas {
		t.Log(tp)
	}
}

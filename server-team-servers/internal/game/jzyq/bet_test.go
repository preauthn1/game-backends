package jzyq

import (
	"testing"
)

func TestBetType_Match(t *testing.T) {

	t.Run("Award Goal Inside", func(t *testing.T) {
		award := GoalInside
		if !Goal.Match(award) {
			t.Errorf("Goal Match Goal Inside want %v, but %v", true, false)
		}
		if !GoalInside.Match(award) {
			t.Errorf("GoalInside Match Goal Left want %v, but %v", true, false)
		}
		if GoalOutside.Match(award) {
			t.Errorf("GoalOutside Match Goal Left want %v, but %v", false, true)
		}

		if Lost.Match(award) {
			t.Errorf("Lost Match Goal Left want %v, but %v", false, true)
		}
		if LostInside.Match(award) {
			t.Errorf("LostInside Match Goal Left want %v, but %v", false, true)
		}
		if LostOutside.Match(award) {
			t.Errorf("LostOutside Match Goal Left want %v, but %v", false, true)
		}
	})

	t.Run("Award Goal Outside", func(t *testing.T) {
		award := GoalOutside
		if !Goal.Match(award) {
			t.Errorf("Goal Match Goal Outside want %v, but %v", true, false)
		}
		if GoalInside.Match(award) {
			t.Errorf("GoalInside Match Goal Outside want %v, but %v", false, true)
		}
		if !GoalOutside.Match(award) {
			t.Errorf("GoalOutside Match Goal Outside want %v, but %v", true, false)
		}

		if Lost.Match(award) {
			t.Errorf("Lost Match Goal Outside want %v, but %v", false, true)
		}
		if LostInside.Match(award) {
			t.Errorf("LostInside Match Goal Outside want %v, but %v", false, true)
		}
		if LostOutside.Match(award) {
			t.Errorf("LostOutside Match Goal Outside want %v, but %v", false, true)
		}
	})

	t.Run("Award Lost Inside", func(t *testing.T) {
		award := LostInside
		if Goal.Match(award) {
			t.Errorf("Goal Match Lost Inside want %v, but %v", false, true)
		}
		if GoalInside.Match(award) {
			t.Errorf("GoalInside Match Lost Inside want %v, but %v", false, true)
		}
		if GoalOutside.Match(award) {
			t.Errorf("GoalOutside Match Lost Inside want %v, but %v", false, true)
		}

		if !Lost.Match(award) {
			t.Errorf("Lost Match Lost Inside want %v, but %v", true, false)
		}
		if !LostInside.Match(award) {
			t.Errorf("LostInside Match Lost Inside want %v, but %v", true, false)
		}
		if LostOutside.Match(award) {
			t.Errorf("LostOutside Match Lost Inside want %v, but %v", false, true)
		}
	})

	t.Run("Award Lost Outside", func(t *testing.T) {
		award := LostOutside
		if Goal.Match(award) {
			t.Errorf("Goal Match Lost Outside want %v, but %v", false, true)
		}
		if GoalInside.Match(award) {
			t.Errorf("GoalInside Match Lost Outside want %v, but %v", false, true)
		}
		if GoalOutside.Match(award) {
			t.Errorf("GoalOutside Match Lost Outside want %v, but %v", false, true)
		}

		if !Lost.Match(award) {
			t.Errorf("Lost Match Lost Outside want %v, but %v", true, false)
		}
		if LostInside.Match(award) {
			t.Errorf("LostInside Match Lost Outside want %v, but %v", false, true)
		}
		if !LostOutside.Match(award) {
			t.Errorf("LostOutside Match Lost Outside want %v, but %v", true, false)
		}
	})

}

func TestBetType_String(t *testing.T) {
	for _, tp := range BetAreas {
		t.Log(tp)
	}
}

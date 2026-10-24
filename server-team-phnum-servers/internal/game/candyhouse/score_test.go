package candyhouse

import "testing"

func TestLogic_UpdateUserScore(t *testing.T) {

	t.Run("No win money", func(tt *testing.T) {
		logic := createBasicTestLogic(4)

		logic.Bet(logic.GameUser, 10)
		logic.GameWinMoney = 0

		logic.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64, coupon int64) {
			if userid != logic.GameUser.UserID {
				tt.Errorf("UserID want %d, but %d", logic.GameUser.UserID, userid)
			}

			if score != -250 {
				tt.Errorf("Score want %d, but %d", -250, score)
			}

			if taxamount != 0 {
				tt.Errorf("TaxAmount want %d, but %d", 0, taxamount)
			}

			if coupon != 0 {
				tt.Errorf("Coupon want %d, but %d", 0, coupon)
			}
		}

		logic.UpdateUserScore(logic.GameUser)

	})

	t.Run("Win money", func(tt *testing.T) {
		logic := createBasicTestLogic(4)

		logic.Bet(logic.GameUser, 10)
		logic.GameWinMoney = 50

		logic.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64, coupon int64) {
			if userid != logic.GameUser.UserID {
				tt.Errorf("UserID want %d, but %d", logic.GameUser.UserID, userid)
			}

			if score != -200 {
				tt.Errorf("Score want %d, but %d", -200, score)
			}

			if taxamount != 0 {
				tt.Errorf("TaxAmount want %d, but %d", 0, taxamount)
			}

			if coupon != 0 {
				tt.Errorf("Coupon want %d, but %d", 0, coupon)
			}
		}

		logic.UpdateUserScore(logic.GameUser)

	})

}

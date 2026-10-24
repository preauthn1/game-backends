package gameadm

import (
	"context"
	"fmt"

	"example.invalid/dig"
	"example.invalid/grpc/codes"
	"example.invalid/grpc/status"

	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"
	"example.invalid/ServerTeam/Servers/internal/game/sml"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
)

// GameAdminSMLServiceServer GameAdminSMLService Server
type GameAdminSMLServiceServer struct {
	pb.UnimplementedGameAdminSMLServer
}

func (s *GameAdminSMLServiceServer) TableDashboard(ctx context.Context, q *pb.SMLQueryTableDashBoard) (*pb.SMLTableDashboard, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	var m *pb.SMLTableDashboard

	if err := app.Context.Invoke(func(f sml.GameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		m = game.MakeDashboard()

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return m, nil
}

func (s *GameAdminSMLServiceServer) TableDashboardStream(q *pb.SMLQueryTableDashBoard, stream pb.GameAdminSML_TableDashboardStreamServer) error {
	if app.Context == nil {
		return status.Error(codes.Aborted, "app.Context not set")
	}

	if err := app.Context.Invoke(func(f sml.GameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		ch := game.SubscripEvent(stream.Context())

		for {
			select {
			case <-stream.Context().Done():
				return nil
			case e := <-ch:
				stream.Send(e)
			}
		}

	}); err != nil {
		if dig.CanVisualizeError(err) {
			return status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return err
	}

	return nil
}

func (s *GameAdminSMLServiceServer) Cheat(ctx context.Context, q *pb.SMLCheatRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f sml.GameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		if game.SpecialRewardCount > 0 {
			return status.Error(codes.FailedPrecondition, "special reward")
		}

		game.Cheat(int(q.KickCnt))

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return m, nil
}

func (s *GameAdminSMLServiceServer) SetSpecialRewardCount(ctx context.Context, req *pb.SetSpecialRewardCountRequest) (*pb.Empty, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f sml.GameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(req.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", req.TableID))
		}

		game := f(t.GameLogic)

		if game.Cheating > 0 {
			return status.Error(codes.FailedPrecondition, "cheating")
		}

		if req.Count > 0 && req.Rate <= 0 {
			return status.Error(codes.InvalidArgument, "rate must be greater than 0")
		}

		game.SpecialRewardCount = req.Count
		game.SpecailRewardRate = req.Rate

		game.EventChan <- game.MakeDashboard()

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return m, nil
}

func (s *GameAdminSMLServiceServer) IncreaseSysPool(ctx context.Context, q *pb.IncreaseSysPoolRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f sml.GameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.Condition.Table)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.Condition.Table))
		}

		game := f(t.GameLogic)

		game.SysPool.UpdateSysPool(q.Amount)

		game.EventChan <- game.MakeDashboard()

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return m, nil
}

func (s *GameAdminSMLServiceServer) AssignNextAward(ctx context.Context, q *pb.SMLAssignNextAwardRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f sml.GameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		// valid award

		var award sml.BetType
		switch q.NextAward {
		default:
			return status.Error(codes.InvalidArgument, fmt.Sprintf("invalid award %d", q.NextAward))
		case pb.SML_BetType_SML_BetTypeGoal:
			award = sml.Goal
		case pb.SML_BetType_SML_BetTypeGoalLeft:
			award = sml.GoalLeft
		case pb.SML_BetType_SML_BetTypeGoalMiddle:
			award = sml.GoalMiddle
		case pb.SML_BetType_SML_BetTypeGoalRight:
			award = sml.GoalRight
		case pb.SML_BetType_SML_BetTypeSave:
			award = sml.Save
		case pb.SML_BetType_SML_BetTypeSaveLeft:
			award = sml.SaveLeft
		case pb.SML_BetType_SML_BetTypeSaveMiddle:
			award = sml.SaveMiddle
		case pb.SML_BetType_SML_BetTypeSaveRight:
			award = sml.SaveRight
		}

		// valid award in sml.AwardOptions
		var exist bool
		for _, v := range sml.AwardOptions {
			if v == award {
				exist = true
				break
			}
		}
		if !exist {
			return status.Error(codes.InvalidArgument, fmt.Sprintf("invalid award %d", q.NextAward))
		}

		// valid award in game.Next
		// if len(game.Game.Next) > 0 {
		// 	return status.Error(codes.FailedPrecondition, "next award already set")
		// }

		// game.Game.Next = append(game.Game.Next, award)

		// reset and add award to game.Game.Next
		game.Game.Next = []sml.BetType{award}

		game.EventChan <- game.MakeDashboard()

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return m, nil
}

func (s *GameAdminSMLServiceServer) SetTableDisabled(ctx context.Context, q *pb.SMLSetTableDisabledRequest) (*pb.Empty, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f sml.GameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		game.Disabled = q.Disabled

		game.EventChan <- game.MakeDashboard()

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return m, nil
}

package gameadm

import (
	"context"
	"fmt"

	"example.invalid/dig"
	"example.invalid/grpc/codes"
	"example.invalid/grpc/status"

	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"
	"example.invalid/ServerTeam/Servers/internal/game/jzyq"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
)

// GameAdminJZYQServiceServer GameAdminJZYQService Server
type GameAdminJZYQServiceServer struct {
	pb.UnimplementedGameAdminJZYQServer
}

func (s *GameAdminJZYQServiceServer) TableDashboard(ctx context.Context, q *pb.JZYQQueryTableDashBoard) (*pb.JZYQTableDashboard, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	var m *pb.JZYQTableDashboard

	if err := app.Context.Invoke(func(f jzyq.GameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminJZYQServiceServer) TableDashboardStream(q *pb.JZYQQueryTableDashBoard, stream pb.GameAdminJZYQ_TableDashboardStreamServer) error {
	if app.Context == nil {
		return status.Error(codes.Aborted, "app.Context not set")
	}

	if err := app.Context.Invoke(func(f jzyq.GameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminJZYQServiceServer) Cheat(ctx context.Context, q *pb.JZYQCheatRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f jzyq.GameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminJZYQServiceServer) SetSpecialRewardCount(ctx context.Context, req *pb.SetSpecialRewardCountRequest) (*pb.Empty, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f jzyq.GameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminJZYQServiceServer) IncreaseSysPool(ctx context.Context, q *pb.IncreaseSysPoolRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f jzyq.GameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminJZYQServiceServer) AssignNextAward(ctx context.Context, q *pb.JZYQAssignNextAwardRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f jzyq.GameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		// valid award

		var award jzyq.BetType
		switch q.NextAward {
		default:
			return status.Error(codes.InvalidArgument, fmt.Sprintf("invalid award %d", q.NextAward))
		case pb.JZYQ_BetType_JZYQ_BetTypeGoal:
			award = jzyq.Goal
		case pb.JZYQ_BetType_JZYQ_BetTypeGoalInside:
			award = jzyq.GoalInside
		case pb.JZYQ_BetType_JZYQ_BetTypeGoalOutside:
			award = jzyq.GoalOutside
		case pb.JZYQ_BetType_JZYQ_BetTypeLost:
			award = jzyq.Lost
		case pb.JZYQ_BetType_JZYQ_BetTypeLostInside:
			award = jzyq.LostInside
		case pb.JZYQ_BetType_JZYQ_BetTypeLostOutside:
			award = jzyq.LostOutside
		}

		// valid award in jzyq.AwardOptions
		var exist bool
		for _, v := range jzyq.AwardOptions {
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

		// reset game.Game.Next
		game.Game.Next = []jzyq.BetType{award}

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

func (s *GameAdminJZYQServiceServer) SetTableDisabled(ctx context.Context, q *pb.JZYQSetTableDisabledRequest) (*pb.Empty, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f jzyq.GameFunc, r *gamelogic.Room) error {

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

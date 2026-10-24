package gameadm

import (
	"context"
	"fmt"

	"example.invalid/dig"
	"example.invalid/grpc/codes"
	"example.invalid/grpc/status"

	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"
	"example.invalid/ServerTeam/Servers/internal/game/roulette"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
)

// GameAdminCJLHLPServiceServer GameAdminCJLHLPService Server
type GameAdminCJLHLPServiceServer struct {
	pb.UnimplementedGameAdminCJLHLPServer
}

func (s *GameAdminCJLHLPServiceServer) TableDashboard(ctx context.Context, q *pb.CJLHLPQueryTableDashBoard) (*pb.CJLHLPTableDashboard, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	var m *pb.CJLHLPTableDashboard

	if err := app.Context.Invoke(func(f roulette.GameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminCJLHLPServiceServer) TableDashboardStream(q *pb.CJLHLPQueryTableDashBoard, stream pb.GameAdminCJLHLP_TableDashboardStreamServer) error {
	if app.Context == nil {
		return status.Error(codes.Aborted, "app.Context not set")
	}

	if err := app.Context.Invoke(func(f roulette.GameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminCJLHLPServiceServer) IncreaseSysPool(ctx context.Context, q *pb.IncreaseSysPoolRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f roulette.GameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminCJLHLPServiceServer) AssignNextAward(ctx context.Context, q *pb.CJLHLPAssignNextAwardRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f roulette.GameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		game.Game.Next = []uint32{q.NextAward}

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

func (s *GameAdminCJLHLPServiceServer) SetTableDisabled(ctx context.Context, q *pb.CJLHLPSetTableDisabledRequest) (*pb.Empty, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f roulette.GameFunc, r *gamelogic.Room) error {

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

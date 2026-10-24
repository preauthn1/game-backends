package gameadm

import (
	"context"
	"fmt"

	"example.invalid/dig"
	"example.invalid/grpc/codes"
	"example.invalid/grpc/status"

	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"
	"example.invalid/ServerTeam/Servers/internal/game/lhd"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

// GameAdminLHDServiceServer GameAdminLHDService Server
type GameAdminLHDServiceServer struct {
	pb.UnimplementedGameAdminLHDServer
}

func (s *GameAdminLHDServiceServer) TableDashboard(ctx context.Context, q *pb.LHDQueryTableDashBoard) (*pb.LHDTableDashboard, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	var m *pb.LHDTableDashboard

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminLHDServiceServer) TableDashboardStream(q *pb.LHDQueryTableDashBoard, stream pb.GameAdminLHD_TableDashboardStreamServer) error {
	if app.Context == nil {
		return status.Error(codes.Aborted, "app.Context not set")
	}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		ch := game.SubscripEvent(stream.Context())

		for {
			select {
			case <-stream.Context().Done():
				// game.UnsubscripEvent(k)
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

func (s *GameAdminLHDServiceServer) Cheat(ctx context.Context, q *pb.LHDCheatRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminLHDServiceServer) SetSpecialRewardCount(ctx context.Context, req *pb.SetSpecialRewardCountRequest) (*pb.Empty, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(req.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", req.TableID))
		}

		game := f(t.GameLogic)

		if game.CheatFlag {
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

func (s *GameAdminLHDServiceServer) IncreaseSysPool(ctx context.Context, q *pb.IncreaseSysPoolRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminLHDServiceServer) AssignNextAward(ctx context.Context, q *pb.LHDAssignNextAwardRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		// valid award

		var award int
		switch q.NextAward {
		default:
			return status.Error(codes.InvalidArgument, fmt.Sprintf("invalid award %d", q.NextAward))
		case pb.LHDAward_LHDAward_LONG:
			award = lhd.LONG
		case pb.LHDAward_LHDAward_TIE:
			award = lhd.HE
		case pb.LHDAward_LHDAward_HU:
			award = lhd.HU
		}

		// valid award in jzyq.AwardOptions
		var exist bool

		allows := []int{lhd.LONG, lhd.HE, lhd.HU}

		for _, v := range allows {
			if v == award {
				exist = true
				break
			}
		}
		if !exist {
			return status.Error(codes.InvalidArgument, fmt.Sprintf("invalid award %d", q.NextAward))
		}

		game.SetNextJiangType(int32(award))

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

func (s *GameAdminLHDServiceServer) SetTableDisabled(ctx context.Context, q *pb.LHDSetTableDisabledRequest) (*pb.Empty, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	m := &pb.Empty{}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

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

func (s *GameAdminLHDServiceServer) SetJackpotEnabled(ctx context.Context, q *pb.LHDJackpotEnabledRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		if game.Jackpot != nil {
			game.Jackpot.AwardPause = !q.Enabled
		}

		game.EventChan <- game.MakeDashboard()

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return &pb.Empty{}, nil
}

func (s *GameAdminLHDServiceServer) AssignJackpotToRandomPlayer(ctx context.Context, q *pb.LHDAssignJackpotRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		if game.Jackpot == nil {
			return status.Error(codes.NotFound, "jackpot not found")
		}

		if !game.Jackpot.Enabled {
			return status.Error(codes.Aborted, "jackpot not enabled")
		}

		if game.Jackpot.AwardPause {
			return status.Error(codes.Aborted, "jackpot award paused")
		}

		if game.Jackpot.Assignment != nil {
			return status.Error(codes.Aborted, "jackpot already assigned")
		}

		if !game.Jackpot.Awardable() {
			return status.Error(codes.Aborted, "jackpot not awardable")
		}

		// assign jackpot to random player
		game.Jackpot.Assignment = &lhd.JackpotAssignment{
			Level: lhd.JackpotLevel(q.Level),
			Type:  lhd.AssignTypeRandom,
		}

		example.invalid("指定Jackpot[%v]給隨機玩家", lhd.JackpotLevel(q.Level))

		game.EventChan <- game.MakeDashboard()

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return &pb.Empty{}, nil
}

func (s *GameAdminLHDServiceServer) AssignJackpotToRecycle(ctx context.Context, q *pb.LHDAssignJackpotRequest) (*pb.Empty, error) {

	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		if game.Jackpot == nil {
			return status.Error(codes.NotFound, "jackpot not found")
		}

		if !game.Jackpot.Enabled {
			return status.Error(codes.Aborted, "jackpot not enabled")
		}

		if game.Jackpot.AwardPause {
			return status.Error(codes.Aborted, "jackpot award paused")
		}

		if game.Jackpot.Assignment != nil {
			return status.Error(codes.Aborted, "jackpot already assigned")
		}

		if !game.Jackpot.Awardable() {
			return status.Error(codes.Aborted, "jackpot not awardable")
		}

		// assign jackpot to random player
		game.Jackpot.Assignment = &lhd.JackpotAssignment{
			Level: lhd.JackpotLevel(q.Level),
			Type:  lhd.AssignTypeRecycle,
		}

		example.invalid("指定Jackpot[%v]回收", lhd.JackpotLevel(q.Level))

		game.EventChan <- game.MakeDashboard()

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return &pb.Empty{}, nil
}

func (s *GameAdminLHDServiceServer) EditJackpotPool(ctx context.Context, q *pb.LHDEditJackpotPoolRequest) (*pb.Empty, error) {
	if app.Context == nil {
		return nil, status.Error(codes.Aborted, "app.Context not set")
	}

	if err := app.Context.Invoke(func(f lhd.LHDGameFunc, r *gamelogic.Room) error {

		t := r.GetTableByNo(q.TableID)

		if t == nil {
			return status.Error(codes.NotFound, fmt.Sprintf("table %d not found", q.TableID))
		}

		game := f(t.GameLogic)

		if game.Jackpot == nil {
			return status.Error(codes.NotFound, "jackpot not found")
		}

		example.invalid("修改 Jackpot 金額 %v", q.FinalPool)

		diff := q.FinalPool - game.Jackpot.Current()

		game.UpdateJackpotPoolNoLink(diff * 10000)

		game.EventChan <- game.MakeDashboard()

		return nil
	}); err != nil {
		if dig.CanVisualizeError(err) {
			return nil, status.Error(codes.Internal, fmt.Sprintf("Invoke error, %v", err))
		}
		return nil, err
	}

	return &pb.Empty{}, nil
}

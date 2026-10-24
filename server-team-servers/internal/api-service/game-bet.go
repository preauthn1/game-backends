package api

import (
	"context"

	pb "example.invalid/ServerTeam/Servers/internal/api-service/pb/apisvc"
	"example.invalid/ServerTeam/Servers/internal/apiserver/apilogic/service"
	"example.invalid/grpc"
	"example.invalid/grpc/codes"
	"example.invalid/grpc/status"
)

// GameBetServiceServer server of game bet service
type GameBetServiceServer struct {
	pb.UnimplementedGameBetServiceServer
}

// PlaceBet 下注扣錢
func (s *GameBetServiceServer) PlaceBet(ctx context.Context, in *pb.PlaceBetReq) (*pb.PlaceBetRes, error) {

	ok, afterAmount, transactionNo := service.GamePlayService.PostPlaceBet(in.AccessToken, in.ServerID, in.TradeID, in.BetAmount, in.IPAddress, in.Memo)

	res := new(pb.PlaceBetRes)

	if !ok {
		return nil, status.Error(codes.Internal, "PlaceBet API Error")
	}

	res.AfterAmount = afterAmount
	res.TransactionNo = transactionNo

	return res, nil
}

func (s *GameBetServiceServer) Summary(ctx context.Context, in *pb.SummaryReq) (*pb.SummaryRes, error) {

	res := new(pb.SummaryRes)
	res.ServerID = in.ServerID

	ok, betAmount, winlossAmount, validAmount := service.GamePlayService.GetBetSummary(in.ServerID, in.Begin, in.End)

	if !ok {
		return nil, status.Error(codes.Internal, "BetSummary API Error")
	}

	res.BetAmount = betAmount
	res.WinLossAmount = winlossAmount
	res.ValidAmount = validAmount

	return res, nil
}

// MockGameBetServiceClient server of game bet service
type MockGameBetServiceClient struct{}

// PlaceBet 下注扣錢
func (s *MockGameBetServiceClient) PlaceBet(ctx context.Context, in *pb.PlaceBetReq, options ...grpc.CallOption) (*pb.PlaceBetRes, error) {
	res := new(pb.PlaceBetRes)
	res.AfterAmount = 0
	res.TransactionNo = "mock"
	return res, nil
}

func (s *MockGameBetServiceClient) Summary(ctx context.Context, in *pb.SummaryReq, opts ...grpc.CallOption) (*pb.SummaryRes, error) {

	res := new(pb.SummaryRes)
	res.ServerID = in.ServerID
	res.BetAmount = 12345
	res.WinLossAmount = 45678

	return res, nil
}

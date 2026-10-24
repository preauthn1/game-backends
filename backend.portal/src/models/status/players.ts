export enum PlayerTypeEnum {
	/**一般账号*/
	Normal = 1,
	/**机器人*/
	Rebot = 2,
}

export enum HDTypeEnum {
	/**PC*/
	PC = 1,
	/**安卓*/
	Android = 2,
	/**苹果*/
	IOS = 3,
}

export enum UserOperateTypeEnum {
	/**封号*/
	Player_LockStatus = 1001,
	/**解封*/
	Player_UnlockStatus = 1002,
	/**登入*/
	User_UserLogin = 7777,
}

export enum WalletTransferTypeEnum {
	Transfer_In = 1,
	Transfer_Out = 2,
}

export enum TransactionTypeEnum {
	/**游戏钱包转入*/
	TransferWallet_To_GameWallet = 1,
	/**游戏钱包转出*/
	TransferWallet_To_PlatformWallet = 2,
	/**游戏结算*/
	BetGame_Settlement = 3,

	ManualDepositWalletAmount = 11,
	
	BetRecord_ManualCancelRecord_RefundWalletManual = 39,

	BetRecord_ChangeSettleInfo = 61,

	EventPrize = 201,

	EventPrize_RedEnvelope = 205,

}

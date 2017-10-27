namespace Plus.Communication.Packets.Incoming
{
    internal static class ClientPacketHeader
    {
        // Handshake
        internal const int InitCryptoMessageEvent = 3392; //316

        internal const int GenerateSecretKeyMessageEvent = 3622; //3847
        internal const int UniqueIdMessageEvent = 3521; //1471
        internal const int SsoTicketMessageEvent = 1989; //1778
        internal const int InfoRetrieveMessageEvent = 2629; //186

        // Avatar
        internal const int GetWardrobeMessageEvent = 3901; //765

        internal const int SaveWardrobeOutfitMessageEvent = 1777; //55

        // Catalog
        internal const int GetCatalogIndexMessageEvent = 3226; //1294

        internal const int GetCatalogPageMessageEvent = 60; //39
        internal const int PurchaseFromCatalogMessageEvent = 3492; //2830
        internal const int PurchaseFromCatalogAsGiftMessageEvent = 1555; //21

        // Navigator

        // Messenger
        internal const int GetBuddyRequestsMessageEvent = 1646; //2485

        // Quests
        internal const int GetQuestListMessageEvent = 2198; //2305        

        internal const int StartQuestMessageEvent = 2457; //1282
        internal const int GetCurrentQuestMessageEvent = 651; //90
        internal const int CancelQuestMessageEvent = 104; //3879

        // Room Avatar
        internal const int ActionMessageEvent = 3268; //3639

        internal const int ApplySignMessageEvent = 3555; //2966
        internal const int DanceMessageEvent = 1225; //645
        internal const int SitMessageEvent = 3735; //1565
        internal const int ChangeMottoMessageEvent = 674; //3515
        internal const int LookToMessageEvent = 1142; //3744
        internal const int DropHandItemMessageEvent = 3296; //1751

        // Room Connection
        internal const int OpenFlatConnectionMessageEvent = 189; //407

        internal const int GoToFlatMessageEvent = 2947; //1601

        // Room Chat
        internal const int ChatMessageEvent = 744; //670

        internal const int ShoutMessageEvent = 697; //2101
        internal const int WhisperMessageEvent = 3003; //878

        // Room Engine

        // Room Furniture

        // Room Settings

        // Room Action

        // Users
        internal const int GetIgnoredUsersMessageEvent = 198;

        // Moderation
        internal const int OpenHelpToolMessageEvent = 1282; //1839

        internal const int CallForHelpPendingCallsDeletedMessageEvent = 3643;
        internal const int ModeratorActionMessageEvent = 760; //781
        internal const int ModerationMsgMessageEvent = 2348; //2375        
        internal const int ModerationMuteMessageEvent = 2474; //1940
        internal const int ModerationTradeLockMessageEvent = 3955; //1160
        internal const int GetModeratorUserRoomVisitsMessageEvent = 3848; //730
        internal const int ModerationKickMessageEvent = 1011; //3589
        internal const int GetModeratorRoomInfoMessageEvent = 1997; //182
        internal const int GetModeratorUserInfoMessageEvent = 2677; //2984
        internal const int GetModeratorRoomChatlogMessageEvent = 3216; //2312
        internal const int ModerateRoomMessageEvent = 500; //3458
        internal const int GetModeratorUserChatlogMessageEvent = 63; //695
        internal const int GetModeratorTicketChatlogsMessageEvent = 1449; //3484
        internal const int ModerationCautionMessageEvent = 2223; //505
        internal const int ModerationBanMessageEvent = 2473; //2595
        internal const int SubmitNewTicketMessageEvent = 1046; //963
        internal const int CloseIssueDefaultActionEvent = 1921;

        // Inventory
        internal const int GetCreditsInfoMessageEvent = 1051; //3697

        internal const int GetAchievementsMessageEvent = 2249; //2931
        internal const int GetBadgesMessageEvent = 2954; //166
        internal const int RequestFurniInventoryMessageEvent = 2395; //352
        internal const int SetActivatedBadgesMessageEvent = 2355; //2752
        internal const int AvatarEffectActivatedMessageEvent = 2658; //129
        internal const int AvatarEffectSelectedMessageEvent = 1816; //628

        internal const int InitTradeMessageEvent = 3399; //3313
        internal const int TradingCancelConfirmMessageEvent = 3738; //2264
        internal const int TradingModifyMessageEvent = 644; //1153
        internal const int TradingOfferItemMessageEvent = 842; //114
        internal const int TradingCancelMessageEvent = 2934; //2967
        internal const int TradingConfirmMessageEvent = 1394; //2399
        internal const int TradingOfferItemsMessageEvent = 1607; //2996
        internal const int TradingRemoveItemMessageEvent = 3313; //1033
        internal const int TradingAcceptMessageEvent = 247; //3374

        // Register
        internal const int UpdateFigureDataMessageEvent = 498; //2560

        // Groups
        internal const int GetBadgeEditorPartsMessageEvent = 3706; //1670

        internal const int GetGroupCreationWindowMessageEvent = 365; //468
        internal const int GetGroupFurniSettingsMessageEvent = 1062; //41
        internal const int DeclineGroupMembershipMessageEvent = 1571; //403
        internal const int JoinGroupMessageEvent = 748; //2615
        internal const int UpdateGroupColoursMessageEvent = 3469; //1443
        internal const int SetGroupFavouriteMessageEvent = 77; //2625
        internal const int GetGroupMembersMessageEvent = 3181; //205

        // Group Forums
        internal const int PostGroupContentMessageEvent = 1499; //477

        internal const int GetForumStatsMessageEvent = 1126; //872

        // Sound

        internal const int RemoveMyRightsMessageEvent = 111; //879
        internal const int GiveHandItemMessageEvent = 2523; //3315
        internal const int GetClubGiftsMessageEvent = 3127; //3302
        internal const int GoToHotelViewMessageEvent = 1429; //3576
        internal const int GetRoomFilterListMessageEvent = 179; //1348
        internal const int GetPromoArticlesMessageEvent = 2782; //3895
        internal const int ModifyWhoCanRideHorseMessageEvent = 3604; //1993
        internal const int RemoveBuddyMessageEvent = 1636; //698
        internal const int RefreshCampaignMessageEvent = 3960; //3544
        internal const int AcceptBuddyMessageEvent = 2067; //45
        internal const int YouTubeVideoInformationMessageEvent = 1295; //2395
        internal const int FollowFriendMessageEvent = 848; //2280
        internal const int SaveBotActionMessageEvent = 2921; //678g
        internal const int LetUserInMessageEvent = 1781; //2356
        internal const int GetMarketplaceItemStatsMessageEvent = 1561; //1203
        internal const int GetSellablePetBreedsMessageEvent = 599; //2505
        internal const int ForceOpenCalendarBoxMessageEvent = 1275; //2879
        internal const int SetFriendBarStateMessageEvent = 3841; //716
        internal const int DeleteRoomMessageEvent = 439; //722
        internal const int SetSoundSettingsMessageEvent = 608; //3820
        internal const int InitializeGameCenterMessageEvent = 1825; //751
        internal const int RedeemOfferCreditsMessageEvent = 2879; //1207
        internal const int FriendListUpdateMessageEvent = 1166; //2664
        internal const int ConfirmLoveLockMessageEvent = 3873; //2082
        internal const int UseHabboWheelMessageEvent = 2148; //2651
        internal const int SaveRoomSettingsMessageEvent = 3023; //2074
        internal const int ToggleMoodlightMessageEvent = 14; //1826
        internal const int GetDailyQuestMessageEvent = 3441; //484
        internal const int SetMannequinNameMessageEvent = 3262; //2406
        internal const int UseOneWayGateMessageEvent = 1970; //2816
        internal const int EventTrackerMessageEvent = 143; //2386
        internal const int FloorPlanEditorRoomPropertiesMessageEvent = 2478; //24
        internal const int PickUpPetMessageEvent = 3975; //2342        
        internal const int GetPetInventoryMessageEvent = 3646; //263
        internal const int InitializeFloorPlanSessionMessageEvent = 3069; //2623
        internal const int GetOwnOffersMessageEvent = 360; //3829
        internal const int CheckPetNameMessageEvent = 3733; //159
        internal const int SetUserFocusPreferenceEvent = 799; //526
        internal const int SubmitBullyReportMessageEvent = 3971; //1803
        internal const int RemoveRightsMessageEvent = 877; //40
        internal const int MakeOfferMessageEvent = 2308; //255
        internal const int KickUserMessageEvent = 1336; //3929
        internal const int GetRoomSettingsMessageEvent = 581; //1014
        internal const int GetThreadsListDataMessageEvent = 2568; //1606
        internal const int GetForumUserProfileMessageEvent = 3515; //2639
        internal const int SaveWiredEffectConfigMessageEvent = 2234; //3431
        internal const int GetRoomEntryDataMessageEvent = 1747; //2768
        internal const int JoinPlayerQueueMessageEvent = 167; //951
        internal const int CanCreateRoomMessageEvent = 2411; //361
        internal const int SetTonerMessageEvent = 1389; //1061
        internal const int SaveWiredTriggerConfigMessageEvent = 3877; //1897
        internal const int PlaceBotMessageEvent = 3770; //2321
        internal const int GetRelationshipsMessageEvent = 3046; //866
        internal const int SetMessengerInviteStatusMessageEvent = 1663; //1379
        internal const int UseFurnitureMessageEvent = 3249; //3846
        internal const int GetUserFlatCatsMessageEvent = 493; //3672
        internal const int AssignRightsMessageEvent = 3843; //3574
        internal const int GetRoomBannedUsersMessageEvent = 2009; //581
        internal const int ReleaseTicketMessageEvent = 3931; //3800
        internal const int OpenPlayerProfileMessageEvent = 3053; //3591
        internal const int GetSanctionStatusMessageEvent = 3209; //2883
        internal const int CreditFurniRedeemMessageEvent = 3945; //1676
        internal const int DisconnectionMessageEvent = 1474; //2391
        internal const int PickupObjectMessageEvent = 1766; //636
        internal const int FindRandomFriendingRoomMessageEvent = 2189; //1874
        internal const int UseSellableClothingMessageEvent = 2849; //818
        internal const int MoveObjectMessageEvent = 3583; //1781
        internal const int GetFurnitureAliasesMessageEvent = 3116; //2125
        internal const int TakeAdminRightsMessageEvent = 1661; //2725
        internal const int ModifyRoomFilterListMessageEvent = 87; //256
        internal const int MoodlightUpdateMessageEvent = 2913; //856
        internal const int GetPetTrainingPanelMessageEvent = 3915; //2088
        internal const int GetSongInfoMessageEvent = 3916; //3418
        internal const int UseWallItemMessageEvent = 3674; //3396
        internal const int GetTalentTrackMessageEvent = 680; //1284
        internal const int GiveAdminRightsMessageEvent = 404; //465
        internal const int GetCatalogModeMessageEvent = 951; //2267
        internal const int SendBullyReportMessageEvent = 3540; //2973
        internal const int CancelOfferMessageEvent = 195; //1862
        internal const int SaveWiredConditionConfigMessageEvent = 2370; //488
        internal const int RedeemVoucherMessageEvent = 1384; //489
        internal const int ThrowDiceMessageEvent = 3427; //1182
        internal const int CraftSecretMessageEvent = 3623; //1622
        internal const int GetGameListingMessageEvent = 705; //2993
        internal const int SetRelationshipMessageEvent = 1514; //2112
        internal const int RequestBuddyMessageEvent = 1706; //3775
        internal const int MemoryPerformanceMessageEvent = 124; //731
        internal const int ToggleYouTubeVideoMessageEvent = 1956; //890
        internal const int SetMannequinFigureMessageEvent = 1909; //3936
        internal const int GetEventCategoriesMessageEvent = 597; //1086
        internal const int DeleteGroupThreadMessageEvent = 50; //3299
        internal const int PurchaseGroupMessageEvent = 2959; //2546
        internal const int MessengerInitMessageEvent = 2825; //2151
        internal const int CancelTypingMessageEvent = 1329; //1114
        internal const int GetMoodlightConfigMessageEvent = 2906; //3472
        internal const int GetGroupInfoMessageEvent = 681; //3211
        internal const int CreateFlatMessageEvent = 92; //3077
        internal const int LatencyTestMessageEvent = 878; //1789
        internal const int GetSelectedBadgesMessageEvent = 2735; //2226
        internal const int AddStickyNoteMessageEvent = 3891; //425
        internal const int ChangeNameMessageEvent = 2709; //1067
        internal const int RideHorseMessageEvent = 3387; //1440
        internal const int InitializeNewNavigatorMessageEvent = 3375; //882
        internal const int SetChatPreferenceMessageEvent = 1045; //2006
        internal const int GetForumsListDataMessageEvent = 3802; //3912
        internal const int ToggleMuteToolMessageEvent = 1301; //2462
        internal const int UpdateGroupIdentityMessageEvent = 1375; //1062
        internal const int UpdateStickyNoteMessageEvent = 3120; //342
        internal const int UnbanUserFromRoomMessageEvent = 2050; //3060
        internal const int UnIgnoreUserMessageEvent = 981; //3023
        internal const int OpenGiftMessageEvent = 349; //1515
        internal const int ApplyDecorationMessageEvent = 2729; //728
        internal const int GetRecipeConfigMessageEvent = 2428; //3654
        internal const int ScrGetUserInfoMessageEvent = 2749; //12
        internal const int RemoveGroupMemberMessageEvent = 1590; //649
        internal const int DiceOffMessageEvent = 1124; //191
        internal const int YouTubeGetNextVideo = 2618; //1843
        internal const int DeleteFavouriteRoomMessageEvent = 3223; //855
        internal const int RespectUserMessageEvent = 3812; //1955
        internal const int AddFavouriteRoomMessageEvent = 3251; //3092
        internal const int DeclineBuddyMessageEvent = 3484; //835
        internal const int StartTypingMessageEvent = 2826; //3362
        internal const int GetGroupFurniConfigMessageEvent = 3902; //3046
        internal const int SendRoomInviteMessageEvent = 1806; //2694
        internal const int RemoveAllRightsMessageEvent = 884; //1404
        internal const int GetYouTubeTelevisionMessageEvent = 1326; //3517
        internal const int FindNewFriendsMessageEvent = 3889; //1264
        internal const int GetPromotableRoomsMessageEvent = 2306; //276
        internal const int GetBotInventoryMessageEvent = 775; //363
        internal const int GetRentableSpaceMessageEvent = 2035; //793
        internal const int OpenBotActionMessageEvent = 3236; //2544
        internal const int OpenCalendarBoxMessageEvent = 1229; //724
        internal const int DeleteGroupPostMessageEvent = 1991; //317
        internal const int CheckValidNameMessageEvent = 2507; //8
        internal const int UpdateGroupBadgeMessageEvent = 1589; //2959
        internal const int PlaceObjectMessageEvent = 1809; //579
        internal const int RemoveGroupFavouriteMessageEvent = 226; //1412
        internal const int UpdateNavigatorSettingsMessageEvent = 1824; //2501
        internal const int CheckGnomeNameMessageEvent = 1179; //2281
        internal const int NavigatorSearchMessageEvent = 618; //2722
        internal const int GetPetInformationMessageEvent = 2986; //2853
        internal const int GetGuestRoomMessageEvent = 2247; //1164
        internal const int UpdateThreadMessageEvent = 2980; //1522
        internal const int AcceptGroupMembershipMessageEvent = 2996; //2259
        internal const int GetMarketplaceConfigurationMessageEvent = 2811; //1604
        internal const int Game2GetWeeklyLeaderboardMessageEvent = 285; //2106
        internal const int BuyOfferMessageEvent = 904; //3699
        internal const int RemoveSaddleFromHorseMessageEvent = 844; //1892
        internal const int GiveRoomScoreMessageEvent = 3261; //336
        internal const int GetHabboClubWindowMessageEvent = 3530; //715
        internal const int DeleteStickyNoteMessageEvent = 3885; //2777
        internal const int MuteUserMessageEvent = 2101; //2997
        internal const int ApplyHorseEffectMessageEvent = 3364; //870
        internal const int GetClientVersionMessageEvent = 4000; //4000
        internal const int OnBullyClickMessageEvent = 254; //1932
        internal const int HabboSearchMessageEvent = 1194; //3375
        internal const int PickTicketMessageEvent = 1807; //3973
        internal const int GetGiftWrappingConfigurationMessageEvent = 1570; //1928
        internal const int GetCraftingRecipesAvailableMessageEvent = 1869; //1653
        internal const int GetThreadDataMessageEvent = 2324; //1559
        internal const int ManageGroupMessageEvent = 737; //2547
        internal const int PlacePetMessageEvent = 1495; //223
        internal const int EditRoomPromotionMessageEvent = 816; //3707
        internal const int GetCatalogOfferMessageEvent = 362; //2180
        internal const int SaveFloorPlanModelMessageEvent = 1936; //1287
        internal const int MoveWallItemMessageEvent = 1778; //609
        internal const int ClientVariablesMessageEvent = 1220; //1600
        internal const int PingMessageEvent = 509; //2584
        internal const int DeleteGroupMessageEvent = 114; //747
        internal const int UpdateGroupSettingsMessageEvent = 2435; //3180
        internal const int GetRecyclerRewardsMessageEvent = 2152; //3258
        internal const int PurchaseRoomPromotionMessageEvent = 1542; //3078
        internal const int PickUpBotMessageEvent = 3058; //644
        internal const int GetOffersMessageEvent = 2817; //442
        internal const int GetHabboGroupBadgesMessageEvent = 3925; //301
        internal const int GetUserTagsMessageEvent = 84; //1722
        internal const int GetPlayableGamesMessageEvent = 1418; //482
        internal const int GetCatalogRoomPromotionMessageEvent = 2757; //538
        internal const int MoveAvatarMessageEvent = 2121; //1737
        internal const int SaveBrandingItemMessageEvent = 2208; //3156
        internal const int SaveEnforcedCategorySettingsMessageEvent = 531; //3413
        internal const int RespectPetMessageEvent = 1967; //1618
        internal const int GetMarketplaceCanMakeOfferMessageEvent = 1552; //1647
        internal const int UpdateMagicTileMessageEvent = 2997; //1248
        internal const int GetStickyNoteMessageEvent = 2469; //2796
        internal const int IgnoreUserMessageEvent = 2374; //2394
        internal const int BanUserMessageEvent = 3009; //3940
        internal const int UpdateForumSettingsMessageEvent = 3295; //931
        internal const int GetRoomRightsMessageEvent = 3937; //2734
        internal const int SendMsgMessageEvent = 2409; //1981
        internal const int CloseTicketMesageEvent = 1080; //50
    }
}
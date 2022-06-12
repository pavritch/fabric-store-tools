namespace InsideFabric.Data
{
    public enum HomewareCategory
    {
        [Category(0, false)] Unknown,
        [Category(1, false)] Ignored,
        [Category(1000, true)] Root,
        [Category(1083, true)] Home_Improvement,
        [Category(1099, true)] Towel_Holders,
        [Category(1084, false)] Andirons,
        [Category(1085, false)] Doors,
        [Category(1134, true)] Faucets,
        [Category(1135, true)] Sinks,
        [Category(1136, true)] Bathroom_Hardware,
        [Category(1137, true)] Bathroom_Fans,
        [Category(1138, true)] Bathroom_Storage,
        [Category(1139, true)] Bathwares,
        [Category(1103, true)] Outdoor,
        [Category(1111, true)] Outdoor_Decor,
        [Category(1112, true)] Patio_Furniture,
        [Category(1108, true)] Garden_Decorations,
        [Category(1109, true)] Garden_Planters,
        [Category(1114, false)] Windchimes,
        [Category(1105, false)] Birdbaths,
        [Category(1104, false)] Birdfeeders,
        [Category(1106, false)] Birdhouses,
        [Category(1113, false)] Watering,
        [Category(1001, true)] Decor,
        [Category(1055, true)] Poufs,
        [Category(1095, true)] Jars,
        [Category(1017, false)] Fireplace,
        [Category(1019, false)] Log_Holders,
        [Category(1020, false)] Tool_Sets,
        [Category(1007, true)] Magazine_Racks,
        [Category(1088, true)] Bowls,
        [Category(1100, true)] Decorative_Trays,
        [Category(1087, true)] Bottles,
        [Category(1004, true)] Faux_Flowers,
        [Category(1110, true)] Garden_Stools,
        [Category(1102, true)] Wine_Racks,
        [Category(1028, true)] Decorative_Pillows,
        [Category(1027, false)] Pillows_Throws_and_Poufs,
        [Category(1029, true)] Throws,
        [Category(1069, true)] Bedding,
        [Category(1002, true)] Clocks,
        [Category(1018, true)] Fire_Screens,
        [Category(1033, true)] Accessories,
        [Category(1012, true)] Candleholders,
        [Category(1015, true)] Lanterns,
        [Category(1011, true)] Candelabras,
        [Category(1013, true)] Candle_Sconces,
        [Category(1010, true)] Candlesticks,
        [Category(1014, true)] Hurricanes,
        [Category(1016, true)] Pillar_Holders,
        [Category(1009, false)] Candles,
        [Category(1089, true)] Centerpieces,
        [Category(1128, true)] Decorative_Boxes,
        [Category(1129, true)] Decorative_Containers,
        [Category(1003, false)] Home_Accents,
        [Category(1006, false)] Decorative_Bowls,
        [Category(1092, true)] Decanters,
        [Category(1008, true)] Vases,
        [Category(1026, true)] Mirrors,
        [Category(1005, true)] Bookends,
        [Category(1107, true)] Baskets,
        [Category(1034, true)] Statues_and_Sculptures,
        [Category(1032, true)] Wall_Art,
        [Category(1030, false)] Window_Treatments,
        [Category(1031, false)] Curtains,
        [Category(1021, false)] Flowers_and_Plants,
        [Category(1025, false)] Wreaths,
        [Category(1022, false)] Floral_Bouquets,
        [Category(1023, false)] Florals,
        [Category(1024, false)] Indoor_Plants,
        [Category(1035, false)] Railings,
        [Category(1140, true)] Faux_Plants,
        [Category(1086, false)] Kitchen_and_Dining,
        [Category(1090, false)] Coasters,
        [Category(1091, false)] Cocktail_Shakers,
        [Category(1093, false)] Food_Storage,
        [Category(1094, false)] Ice_Buckets,
        [Category(1096, false)] Pitchers,
        [Category(1097, false)] Plates,
        [Category(1098, false)] Stands,
        [Category(1101, false)] Utensils,
        [Category(1115, true)] Lighting,
        [Category(1123, true)] Floor_Lamps,
        [Category(1124, true)] Table_Lamps,
        [Category(1141, true)] Vanity_Lights,
        [Category(1125, true)] Wall_Lamps,
        [Category(1117, true)] Pendants,
        [Category(1119, true)] Chandeliers,
        [Category(1121, true)] Sconces,
        [Category(1116, false)] Bulbs,
        [Category(1118, false)] Chains,
        [Category(1122, false)] Lamp_Parts,
        [Category(1120, false)] Mounts,
        [Category(1126, false)] Shades,
        [Category(1037, true)] Furniture,
        [Category(1066, true)] Bedroom_Furniture,
        [Category(1068, true)] Bedroom_Sets,
        [Category(1071, true)] Nightstands,
        [Category(1070, true)] Dressers,
        [Category(1067, true)] Beds,
        [Category(1060, true)] Office_Furniture,
        [Category(1061, true)] Desks,
        [Category(1062, true)] Desk_Chairs,
        [Category(1063, false)] Filing,
        [Category(1065, false)] Supplies,
        [Category(1048, true)] Living_Room_Furniture,
        [Category(1050, true)] Console_Tables,
        [Category(1057, true)] Side_Tables,
        [Category(1056, true)] Side_Chairs,
        [Category(1052, true)] End_Tables,
        [Category(1051, true)] Coffee_Tables,
        [Category(1131, true)] Shelves,
        [Category(1053, true)] Love_Seats,
        [Category(1058, true)] Sofas,
        [Category(1054, true)] Ottomans,
        [Category(1059, true)] TV_Stands,
        [Category(1049, false)] Coat_Racks,
        [Category(1133, true)] Cabinets,
        [Category(1038, true)] Accent_Furniture,
        [Category(1040, true)] Accent_Chairs,
        [Category(1043, true)] Accent_Tables,
        [Category(1039, true)] Accent_Cabinets,
        [Category(1042, true)] Accent_Stools,
        [Category(1046, true)] Decorative_Trunks,
        [Category(1041, true)] Accent_Shelves,
        [Category(1045, true)] Bookcases,
        [Category(1044, true)] Benches,
        [Category(1047, false)] Room_Screens,
        [Category(1074, true)] Kitchen_Furniture,
        [Category(1064, true)] Gathering_Table,
        [Category(1081, true)] Island,
        [Category(1079, true)] Dining_Sets,
        [Category(1078, true)] Dining_Chairs,
        [Category(1080, true)] Dining_Tables,
        [Category(1075, true)] Bar_Carts,
        [Category(1076, true)] Bar_Stools,
        [Category(1077, true)] Bar_Tables,
        [Category(1082, true)] Sideboards,
        [Category(1072, true)] Bathroom_Furniture,
        [Category(1073, true)] Vanities,
        [Category(1127, false)] Storage,
        [Category(1130, false)] Luggage,
        [Category(1132, false)] Wastebaskets
    }
}
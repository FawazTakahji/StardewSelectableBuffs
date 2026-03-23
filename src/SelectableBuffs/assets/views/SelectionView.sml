<lane orientation="vertical" horizontal-content-alignment="middle">
    <banner background={@Mods/StardewUI/Sprites/BannerBackground}
            background-border-thickness="48,0"
            padding="12"
            text={:Title} />

    <frame margin="0,16,0,0"
           padding="32,24"
           background={@Mods/StardewUI/Sprites/ControlBorder}>
        <lane orientation="horizontal" padding="16, 0, 0, 0">
            <image *repeat={:Options}
                   focusable="true"
                   click=|^SelectOption(Key)|
                   margin="0, 0, 16, 0"
                   sprite={:Sprite}
                   tooltip={:Title} />
        </lane>
    </frame>
</lane>
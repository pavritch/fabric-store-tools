jQuery(function($) {
    "use strict";

/**
 * Context Menu Class
 * 
 * @param {element} element The element to bind the menu to
 * @param {type} options
 * @returns {ContextMenu}
 */
var ContextMenu = function( element, options ) 
{
    this.element      = element;
    this.settings     = options;
    this.items        = new ContextMenu.items(this);
    this.selector     = new ContextMenu.selector(this);
    this.events       = new ContextMenu.events(this);
    this.position     = new ContextMenu.position(this);
    this.arrow        = new ContextMenu.arrow(this);
    this.container    = new ContextMenu.container(this);
    
    this.build();
};

/**
 * Build the menu
 * 
 * Called when the widget is created, and later when changing options
 */
ContextMenu.prototype.build = function() 
{
    this.container.build();
    this.events.bind();
};

/**
 * 
 * @returns {undefined}
 */
ContextMenu.prototype.destroy = function() 
{
    this.container.destroy();
    this.events.unbind();
};

/**
 * 
 * @param {type} e
 * @returns {undefined}
 */
ContextMenu.prototype.show = function(e) 
{   
    this.container.show(e);
};

/**
 * 
 * @returns {undefined}
 */
ContextMenu.prototype.hide = function(e) 
{
    this.container.hide(e);
};

/**
 * 
 * @param {type} hook
 * @param {type} e
 * @returns {undefined}
 */
ContextMenu.prototype.trigger = function( hook, e ) 
{
    var func = this.settings.hooks[hook];
    if(typeof func === 'function')
    {
        func.call(null,e);
    }
};

/**
 * Default plugin options
 */
ContextMenu.defaults = {
    maxHeight:  null,
    minHeight:  null,
    maxWidth:   null,
    minWidth:   160,
    height:     null,
    width:      null,
    class:      null,   // Optional CSS class
    event:      'contextmenu', // Or [click|dblclick|hover|focus]
    selector:   null, // Repeat the selector here for dynamic binding
    hooks:      {show:null,hide:null,position:null},
    position:   {my:'left-25 top+5', at: 'mouse', children: false}, // Or {my:'left top', at: 'center bottom'} (using jQuery UI position
    autoHide:   false, // Or delay time in miliseconds
    transition: { speed: 0, type: 'none' },
    appendTo:   document.body,
    arrow:      'auto', // Or bottom, left, right, top (false for no arrow)
    items:      [],     // Or a function that takes the current event as an argument and returns a list of items
    click:      null,   // General purpose click function (overriden by item's click function)
    hover:      null    // General purpose hover function (overriden by item's hover function)
};

ContextMenu.items = function( parent )
{
    this.parent = parent;
};

ContextMenu.items.prototype.build = function( e )
{
    var settings = this.parent.settings;
    
    // Dynamically build the items
    if( this.isDynamic() )
    {
        // For the items to build dynamically, e must be set
        if( typeof e === 'undefined' ) return;
        
        // Remove the previously built menu
        if( this.isBuilt() ) 
        {
            this.root.element.remove();
            this.root = undefined;
        }
    }
    
    // Don't build menu if it has already been built
    if(this.isBuilt()) return;
        
    // Build nodes dynamically if a function was provided
    if(this.isDynamic())
    {
        this.nodes = settings.items.call(null,e);
    }
    // Or save a copy of the items array to leave the original set intact
    else
    {
        this.nodes = this.cloneItems( settings.items );
    }
    
    this.postProcess( this.nodes );
    ContextMenu.items.resetIDs();
    this.root = new ContextMenu.menu( this.getAll(), this.parent );
};

// Post process items after they have been created
ContextMenu.items.prototype.postProcess = function( items )
{
    for( var i = 0; i < items.length; i++ )
    {
        var item = items[i];
        
        // Set general purpose click/hover event if the item does not have one set
        if('item' === item.type || 'checkbox' === item.type)
        {
            if(!item.click) item.click = this.parent.settings.click;
            if(!item.hover) item.hover = this.parent.settings.hover;
        }
        if(item.hasSubmenu && item.hasSubmenu())
        {
            this.postProcess(item.submenu);
        }
    }
    
};

ContextMenu.items.prototype.cloneItems = function( items )
{
    var newItems = [];
    for( var i = 0; i < items.length; i++ )
    {
        // Copy each object
        newItems.push($.extend({},items[i]));
    }
    return newItems;
};

ContextMenu.items.prototype.getAll = function()
{
    if(typeof this.nodes !== 'undefined')
    {
        return this.nodes;
    }
    return [];
};

ContextMenu.items.prototype.isDynamic = function()
{
    return typeof this.parent.settings.items === 'function';
};

ContextMenu.items.prototype.isBuilt = function()
{
    return typeof this.root !== 'undefined';
};

// Used by selectable menu items like 'item' or 'checkbox' to generate a unique id
ContextMenu.items.generateID = function()
{
    if( typeof this.currentID === 'undefined' )
    {
        this.currentID = 0;
    }
    return this.currentID++;
};

ContextMenu.items.resetIDs = function()
{
    this.currentID = 0;
};

/**
 * A utility class for selecting and navigating between menu items.
 */
ContextMenu.selector = function( parent )
{
    this.parent = parent;
};

/**
 * Select a menu item
 * 
 * This function traverses the entire menu (including children) until the item 
 * with the matching id is found. If it is found, this.activeItem is set to the
 * matched item, and this.activeParents is set to the array of parent items
 * leading to the item.
 * 
 * @param {number} itemID The item id
 * @param {ContextMenu.menu} menu The containing menu (leave blank to use the root menu)
 * @param {array} parents An array containing the item's parent items, from furthest to closest
 * @returns {Boolean}
 */
ContextMenu.selector.prototype.select = function( itemID, menu, parents )
{
    if(typeof menu === 'undefined') menu = this.parent.items.root;

    for( var i = 0; i < menu.items.length; i++ )
    {
        var item = menu.items[i].instance;

        if(item.getID && item.getID() === itemID)
        {
            this.deselect(); // Prevent double selection
            
            if( $.isArray(parents) && parents.length > 0 )
            {
                $(parents).each(function(){
                    this.element.addClass('active');
                    this.submenu.show();
                });
                this.activeParents = parents;
            }
            else
                this.activeParents = undefined;
            
            item.element.addClass('active');
            this.activeItem = item;
            
            return true;
        }
        if(item.hasSubmenu && item.hasSubmenu())
        {
            // Add current item to active parents
            if( typeof parents === 'undefined' )
            {
                parents = [item];
            }
            else
            {
                parents.push(item);
            }
            
            // return true if the item is a in the submenu
            if(this.select(itemID, item.submenu, parents))
            {
                return true;
            }
            // Otherwise, remove the last added parent and continue
            parents.pop();
        }
    }
    return false;
};

/**
 * Deselect current item
 */
ContextMenu.selector.prototype.deselect = function()
{
    if(this.activeItem)
    {
        this.activeItem.element.removeClass('active');
        
        // Hide the submenu of the current element
        if( this.activeItem.hasSubmenu && this.activeItem.hasSubmenu() )
            this.activeItem.submenu.hide();
        
        this.activeItem = undefined;
        $(this.activeParents).each(function(){
            this.element.removeClass('active');
            this.submenu.hide();
        });
        this.activeParents = [];
    }
};

/**
 * Get the currently selected item
 * @returns {item}
 */
ContextMenu.selector.prototype.getSelection = function()
{
    return this.activeItem;
};

ContextMenu.selector.prototype.getFirstItem = function( menu )
{
    var items = menu ? menu.items : this.parent.items.root.items;
    for( var i = 0; i < items.length; i++ )
    {
        var item = items[i].instance;
        if( item.isDisabled && !item.isDisabled() )
        {
            return item;
        }
    }
    throw "This context menu has no selectable items";
};

/**
 * Get the next item in the current menu, ignoring children 
 * items and disabled items
 * @returns {Number} the item's id
 */
ContextMenu.selector.prototype.getNextItem = function()
{
    var menu = this.parent.items.root;
    if( !this.activeItem ) return this.getFirstItem();
    if(this.activeParents) menu = this.activeParents[this.activeParents.length-1].submenu;
    
    for( var i = 0; i < menu.items.length; i++ )
    {
        var item = menu.items[i].instance;
        if(item.id > this.activeItem.id && item.isDisabled && !item.isDisabled())
        {
            return item;
        }
    }
    return this.activeItem;
};

/**
 * Get the previous item in the current menu, ignoring children 
 * items and disabled items
 * @returns {Number} the item's id
 */
ContextMenu.selector.prototype.getPrevItem = function()
{
    var menu = this.parent.items.root;
    if( !this.activeItem ) return this.getFirstItem();
    if(this.activeParents) menu = this.activeParents[this.activeParents.length-1].submenu;
    
    for( var i = menu.items.length-1; i >= 0; i-- )
    {
        var item = menu.items[i].instance;
        if(item.id < this.activeItem.id && item.isDisabled && !item.isDisabled())
        {
            return item;
        }
    }
    return this.activeItem;
};

/**
 * Get the id of the first child of the currently active item
 * @returns {Number} the item's id
 */
ContextMenu.selector.prototype.getFirstChildItem = function()
{
    if( !this.activeItem ) return this.getFirstItem();
    if(this.activeItem.hasSubmenu && this.activeItem.hasSubmenu())
    {
        return this.getFirstItem(this.activeItem.submenu);
    }
    return this.activeItem;
};

/**
 * Get the id of the parent item of the currently active item
 * @returns {Number} the item's id
 */
ContextMenu.selector.prototype.getImmediateParentItem = function()
{
    if( !this.activeItem ) return this.getFirstItem();
    if( !this.activeParents ) return this.activeItem;
    return this.activeParents[this.activeParents.length-1];
};

/**
 * A wrapper for menu items.
 * 
 * @param {type} items
 * @returns {undefined}
 */
ContextMenu.menu = function( items, ctxMenu )
{
    this.ctxMenu = ctxMenu;
    this.build( items );
};

ContextMenu.menu.prototype.build = function( items )
{
    this.items = [];
    this.element = $('<ul>').addClass('contextmenu-menu');
    for( var i =0; i < items.length; i++ )
    {
        this.addItem( items[i] );
    }
};

ContextMenu.menu.prototype.addItem = function( item )
{
    var instance = new ContextMenu.ui[item.type](item,this.ctxMenu);
    this.items.push( $.extend({},item,{instance: instance}) );
    $(this.element).append( instance.element );
};

// Show and reposition if necessary
ContextMenu.menu.prototype.show = function()
{
    this.element.css({display:'block'});
    
    var w = this.element.outerWidth(),
        h = this.element.outerHeight(),
        t = this.element.offset().top - $(window).scrollTop(),
        l = this.element.offset().left;

    if( l+w > $(window).width() )
    {
        this.element.css({left:'-100%'});
    }
    if( t+h > $(window).height() )
    {
        this.element.css({top:$(window).height()-(t+h)});
    }
};

ContextMenu.menu.prototype.hide = function()
{
    // Hide and reset position
    this.element.css({display:'none',top:'0',left:'100%'});
};

/**
 * A wrapper around all menu elements
 * 
 * @param {ContextMenu} parent
 */
ContextMenu.container = function( parent )
{
    this.parent = parent;
    this.createElement();
    
    // Don't set autoHide for hover event, since that event implements its own auto hiding functionality
    if( false !== this.parent.settings.autoHide && 'hover' !== this.parent.settings.event )
    {
        this.setAutoHide();
    }
    
    // Reposition menu when resizing
    $(window).on('resize', function(){
        parent.position.onResize();
    });
};

ContextMenu.container.prototype.createElement = function()
{
    var p = this.parent,
        t = p.settings.transition;
    this.element = $('<div>')
            .attr( 'id', "contextmenu" )
            .css({'animation-duration':t.speed+'ms'})
            .addClass('contextmenu-transition-'+t.type)
            .addClass(p.settings.class);
};

ContextMenu.container.prototype.build = function( e )
{
    var items = this.parent.items;
    items.build(e);
    
    // Don't build menu if there are no items
    if( items.isBuilt() && items.getAll().length )
    {
        $(this.element).append(this.parent.items.root.element);
        this.setMenuDimensions();
        this.isBuilt = true;
    }
    else
    {
        this.isBuilt = false;
    }
};

ContextMenu.container.prototype.setMenuDimensions = function()
{
    var s = this.parent.settings,
        m = this.parent.items.root.element;

    if(s.minHeight) m.css({'min-height':s.minHeight});
    if(s.minWidth) m.css({'min-width':s.minWidth});
    if(s.maxHeight) m.css({'max-height':s.maxHeight,'overflow-y':'auto','overflow-x':'hidden'});
    if(s.maxWidth) m.css({'max-width':s.maxWidth});
    if(s.height) m.css({'height':s.height,'overflow-y':'auto','overflow-x':'hidden'});
    if(s.width) m.css({'width':s.width});
};

ContextMenu.container.prototype.show = function(e)
{
    if(this.parent.items.isDynamic()) this.build(e);
    if(!this.isBuilt) return;
    
    this.cancelHide();
    
    if( this.isVisible() )
    {
        this.parent.position.set(e);
        return;
    }
    
    $(this.parent.settings.appendTo).append(this.element);
    this.parent.position.set(e);
    this.element.addClass('contextmenu-visible');
    this.parent.trigger('show',e);
};

ContextMenu.container.prototype.hide = function( e, delay )
{
    if( !this.isVisible() ) return;

    var ctx = this.parent;
    this.timeout = setTimeout(function(){
        
        ctx.container.element.removeClass('contextmenu-visible');
    
        // Hide menu after transition has finished
        ctx.container.detachTimeout = setTimeout(function(){
            ctx.selector.deselect();
            ctx.container.element.detach(); // Detach and keep events
        },ctx.settings.transition.speed);

        ctx.trigger('hide',e);
    },delay||0);
};

ContextMenu.container.prototype.cancelHide = function()
{
    clearTimeout(this.detachTimeout);
    clearTimeout(this.timeout);
};

/**
 * 
 * @returns {ContextMenu.prototype@pro;menu@pro;element@call;hasClass|Boolean}
 */
ContextMenu.container.prototype.isVisible = function() 
{
    return this.parent.items.isBuilt() && this.element.hasClass('contextmenu-visible');
};

ContextMenu.container.prototype.setAutoHide = function() 
{
    // this.timeout is also used outside of this function
    var ctx = this.parent;
    this.element.hover(
        function(e){
            ctx.container.cancelHide();
        },
        function(e){
            ctx.container.hide(e,ctx.settings.autoHide);
        }
    );
};

ContextMenu.container.prototype.destroy = function()
{
    this.element.remove();
};


/**
 * Event binding utility class
 * 
 * @param {ContextMenu} parent
 */
ContextMenu.events = function( parent ) 
{
    this.parent = parent;
};

ContextMenu.events.prototype.bind = function() 
{
    this[this.parent.settings.event]();
    this.setKeyboardNavigation();
};

ContextMenu.events.prototype.unbind = function() 
{
    // TODO: Also remove events bound to the html element itself
    if( this.parent.settings.selector !== null )
    {
        $('html').off('click contextmenu mouseenter mouseleave dblclick focusin', this.parent.settings.selector);
    }
};

ContextMenu.events.prototype.hover = function() 
{
    var ctx = this.parent,
        selector = ctx.settings.selector;
    
    $(selector ? 'html' : ctx.element).on({
        mouseenter: function (e) {
            if( ctx.events.targetIsElement(e.target) )
            {
                ctx.show(e);
            }
        },
        mouseleave: function (e) {
            // Hide if not hovering on the menu itself
            if( !$(e.toElement).closest(ctx.container.element).length )
            {
                ctx.container.hide(e,ctx.settings.autoHide);
            }
        },
        mousemove: function (e) {
            // Reposition menu on mouse move (for cases when position.children is set)
            if( ctx.events.targetIsElement(e.target) && ctx.settings.position.children)
            {
                ctx.show(e);
            }
        }
    }, selector);
    
    // Hide the menu when the mouse leaves it, but only if it doesn't
    // go back to the triggering element
    this.parent.container.element.hover(
        function(e){
            ctx.container.cancelHide();
        },
        function(e){
            if(!$(e.toElement).closest(ctx.element).length)
            {
                ctx.container.hide(e,ctx.settings.autoHide);
            }
        }
    );
    
    $('html').on('click contextmenu', function(e){ctx.events.offEvent(e);});
};

ContextMenu.events.prototype.contextmenu = function() 
{
    var ctx = this.parent;
    
    $('html').on('contextmenu', ctx.settings.selector, function(e){ctx.events.onEvent(e);});
    $('html').on('click', function(e){ctx.hide(e);}); // A click anywhere hides the menu
};

ContextMenu.events.prototype.click = function() 
{
    var ctx  = this.parent;
    
    $('html').on('click', ctx.settings.selector, function(e){ctx.events.onEvent(e);});
    $('html').on('click contextmenu', function(e){ctx.events.offEvent(e);});
};

ContextMenu.events.prototype.focus = function() 
{
    var ctx  = this.parent;
    
    $('html').on('focusin', ctx.settings.selector, function(e){ctx.events.onEvent(e);});
    $('html').on('click contextmenu', function(e){ctx.events.offEvent(e);});
};

ContextMenu.events.prototype.dblclick = function() 
{
    var ctx  = this.parent;
    
    $('html').on('dblclick', ctx.settings.selector, function(e){ctx.events.onEvent(e);});
    $('html').on('click contextmenu', function(e){ctx.events.offEvent(e);});
};

ContextMenu.events.prototype.onEvent = function(e)
{
    var ctx  = this.parent;
    if( ctx.events.targetIsElement(e.target) )
    {
        e.preventDefault();
        ctx.show(e);
    }
    else
    {
        ctx.hide(e);
    }
}

ContextMenu.events.prototype.offEvent = function(e)
{
    if( !this.parent.events.targetIsElement(e.target) )
    {
        this.parent.hide(e);
    }
}

ContextMenu.events.prototype.targetIsElement = function( target ) 
{
    return $(target).closest( this.parent.settings.selector || this.parent.element ).length;
};

ContextMenu.events.prototype.setKeyboardNavigation = function()
{
    var p = this.parent;
            
    // Also catch keyup to prevent it's default behaviour
    $(document).on('keydown keyup',function(e){
        if(p.container.isVisible())
        {
            var selector = p.selector,
                c = e.which,
                prevItem = selector.getSelection(),
                nextItem;
            
            // Enter key pressed
            if(c === 13 && prevItem)
            {
                e.preventDefault();
                prevItem.element.trigger('click');
                return;
            }
            
            // Arrow keys
            if(c === 37 || c === 38 || c === 39 || c === 40)
            {
                e.preventDefault();
                
                // Only select on keydown
                if(e.type==='keyup') return;
                
                switch(c)
                {
                    case 37: // Left key
                        nextItem = selector.getImmediateParentItem();
                        break;
                    case 38: // Up key
                        nextItem = selector.getPrevItem();
                        break;
                    case 39: // Right key
                        nextItem = selector.getFirstChildItem();
                        break;
                    case 40: // Down key
                        nextItem = selector.getNextItem();
                        break;
                }
                
                // If the selections has changed, select the new item and trigger its hover event
                if(!prevItem || prevItem.getID() !== nextItem.id)
                {
                    if(prevItem) prevItem.element.trigger('mouseleave'); // This also triggers selector.deselect()
                    nextItem.element.trigger('mouseenter'); // This also triggers selector.select()
                }
                
                // Scroll the continer to show the item if 'maxHeight' was set
                if( null !== p.settings.maxHeight )
                {
                    var $item = nextItem.element,
                        $root = p.items.root.element,
                        itemHeight = $item.outerHeight(),
                        rootHeight = $root.outerHeight();
                    
                    // Item is below the bottom of the container
                    if( $item.offset().top + itemHeight > $root.offset().top + rootHeight )
                        $root.scrollTop( $root.scrollTop() + itemHeight );
                    
                    // Item is above the top of the container
                    if( $item.offset().top < $root.offset().top )
                        $root.scrollTop( $root.scrollTop() - itemHeight );
                }
            }
        }
    });
};

/**
 * Menu positioning control class 
 * 
 * @param {ContextMenu} parent
 */
ContextMenu.position = function( parent ) 
{
    this.parent = parent;
};

ContextMenu.position.prototype.set = function(e) 
{
    this.element = this.getElement(e);
    this.offset  = {x: (e.pageX - $(this.element).offset().left), y: (e.pageY - $(this.element).offset().top)};
    this.target  = e.target;
    this.mouse   = {x:e.pageX, y:e.pageY};

    this.$position();
    this.parent.selector.deselect();
    this.parent.trigger('position',e);
};

ContextMenu.position.prototype.onResize = function() 
{
    if( this.parent.container.isVisible() )
    {
        this.$position();
    }
};

ContextMenu.position.prototype.$position = function() 
{
    if(typeof this.element === 'undefined') throw "Must use ContextMenu.position.set() prior to $position";
    
    var ctx      = this.parent,
        position = ctx.settings.position,
        mouse    = position.at === 'mouse',
        my       = position.my,
        at       = mouse ? 'left+'+this.offset.x+' '+'top+'+this.offset.y : position.at;

    $(this.parent.container.element).position({
        my: my,
        at: at,
        of: position.children !== false && $(this.target).closest(position.children).length ? $(this.target).closest(position.children) : this.element,
        collision: 'fit',
        using: function( pos, rel ) {
            rel.element.element.css({top:pos.top,left:pos.left});
            ctx.position.placeArrow( rel.element, rel.target );
        }
    });
};

ContextMenu.position.prototype.placeArrow = function( element, target ) 
{
    var ctx      = this.parent,
        settings = ctx.settings,
        arrow    = ctx.arrow,
        mouse    = ctx.settings.position.at === 'mouse';
            
    if( false !==  settings.arrow )
    {
        if('auto' === settings.arrow)
        {
            arrow.setPosition( mouse ? arrow.calcCoordsRelativePosition( this.mouse ) : arrow.calcTargetRelativePosition( element, target ) );
        }
        else
        {
            arrow.setPosition(settings.arrow);
        }
    }
}

/**
 * Returns the correct target element, even if the target element has been created dynamically
 * 
 * @param {event} e 
 * @returns {element}
 */
ContextMenu.position.prototype.getElement = function(e) 
{
    // If the selector argument is set, use it to check if the event's target element match
    if( null !== this.parent.settings.selector )
    {
        return $(e.target).closest(this.parent.settings.selector)
    }
    return this.parent.element;
}

/**
 * Implements the menu's arrow element
 * 
 * @param {ContextMenu} parent
 */
ContextMenu.arrow = function( parent ) 
{
    this.parent = parent;
};

ContextMenu.arrow.prototype.setPosition = function( position ) 
{
    if(!this.parent.settings.arrow.match()) return;
    
    var positions = ['top','left','right','bottom'],
        prefix    = 'contextmenu-arrow-',
        classes   = prefix+positions.join(' '+prefix),
        container = this.parent.container.element;
        
    $(container)
        .removeClass(classes)
        .addClass(prefix+position);
};

/**
 * Calculate where the menu is relative to the mouse cursor and return
 * the position where the arrow should be placed
 */
ContextMenu.arrow.prototype.calcCoordsRelativePosition = function( coords ) 
{
    var mx = coords.x,
        my = coords.y,
        el = $(this.parent.container.element),
        ml = el.offset().left,
        mr = ml+el.width(),
        mt = el.offset().top,
        mb = mt+el.height();

    if( mx <= mr && mx >= ml )
    {
        if( my <= mt ) return 'top';
        if( my >= mb ) return 'bottom';
    }
    
    if( my <= mb && my >= mt )
    {
        if( mx <= ml ) return 'left';
        if( mx >= mr ) return 'right';
    }
}

/**
 * Calculate where the menu is relative to the target element and return
 * the position where the arrow should be placed
 */
ContextMenu.arrow.prototype.calcTargetRelativePosition = function( element, target )
{
    var el = element.left,
        er = element.left+element.width,
        et = element.top,
        eb = element.top+element.height,
        tl = target.left,
        tr = target.left+target.width,
        tt = target.top,
        tb = target.top+target.height;
        
    if( el > tl && el < tr && et > tt && et < tb ) return 'top'; // Menu is inside the target element
    if( eb <= tt ) return 'bottom'; // Menu is above the target element
    if( et >= tb ) return 'top'; // Menu is below the target element
    if( el >= tr ) return 'left'; // Menu is to the right of the target element
    if( er <= tl ) return 'right'; // Menu is to the left of the target element
};

/**
 * ContextMenu UI namespace
 */
ContextMenu.ui = function() {};

ContextMenu.ui.title = function( settings ) 
{
    this.element = $('<li>').addClass('contextmenu-title').html( settings.text );
};

/**
 * Divider Menu Item
 * 
 * @param {object} settings
 */
ContextMenu.ui.divider = function( settings ) 
{
    this.element = $('<li>').addClass('contextmenu-divider');
};

/**
 * General Menu Item
 * 
 * @param {object} settings
 */
ContextMenu.ui.item = function( settings, ctxMenu ) 
{
    this.settings = settings;
    this.ctxMenu = ctxMenu;
    this.id = ContextMenu.items.generateID(); // Item ID, not HTML ID
    this.icon = $('<i>').addClass(settings.icon);
    this.text = $('<span>').addClass('contextmenu-item-text').html(settings.text);
    this.element = $('<li>').addClass('contextmenu-item')
                            .attr('data-item-id', this.id)
                            .append(this.icon)
                            .append(this.text);
    
    if(settings.disabled) this.disable();
    if(typeof settings.id !== 'undefined') this.element.attr('id',settings.id);
    this.createSubmenu();
    this.bindEvents();
};

ContextMenu.ui.item.prototype.disable = function()
{
    this.element.addClass('contextmenu-disabled');
};

ContextMenu.ui.item.prototype.isDisabled = function()
{
    return this.element.hasClass('contextmenu-disabled');
};

ContextMenu.ui.item.prototype.bindEvents = function()
{
    if(this.settings.disabled) return;
    
    var self = this,
        obj = $.extend({},self.settings,{instance: self});
    
    this.element.on('mouseenter',function(e){
        e.preventDefault();
        e.stopPropagation();
        
        if( !self.isDisabled() )
            self.ctxMenu.selector.select(self.getID());
        
        if( self.hasSubmenu() )
            self.submenu.show();
        
        if( typeof self.settings.hover === 'function' )
            self.settings.hover.call(obj,e);
    });
    
    this.element.on('mouseleave',function(e){
        e.preventDefault();
        e.stopPropagation();
        
        self.ctxMenu.selector.deselect();
        
        if( self.hasSubmenu() )
            self.submenu.hide();
    });
    
    this.element.on('click',function(e){
        if( typeof self.settings.click === 'function' )
            self.settings.click.call(obj,e);
        
        if( self.hasSubmenu() )
            self.submenu.hide();
    });
};

ContextMenu.ui.item.prototype.createSubmenu = function()
{
    if(this.settings.disabled) return;
    if( this.hasSubmenu() )
    {
        this.submenu = new ContextMenu.menu( this.settings.items, this.ctxMenu );
        this.element.addClass('contextmenu-submenu').append( this.submenu.element );
    }
};

ContextMenu.ui.item.prototype.hasSubmenu = function()
{
    return Array.isArray( this.settings.items ) && this.settings.items.length > 0;
};

ContextMenu.ui.item.prototype.getID = function()
{
    return this.id;
};

/**
 * Checkbox Menu Item
 * 
 * @param {object} settings
 */
ContextMenu.ui.checkbox = function( settings, ctxMenu ) 
{
    this.settings = settings;
    this.ctxMenu = ctxMenu;
    this.id = ContextMenu.items.generateID(); // Item ID, not HTML ID
    this.icon = $('<i>').addClass(settings.icon);
    this.text = $('<span>').addClass('contextmenu-item-text').html(settings.text);
    this.element = $('<li>').addClass('contextmenu-checkbox')
                            .attr('data-item-id', this.id)
                            .append(this.icon)
                            .append(this.text);

    if(settings.disabled) this.disable();
    if(settings.checked) this.check();
    if(typeof settings.id !== 'undefined') this.element.attr('id',settings.id);
    this.registerEventListeners();
};

ContextMenu.ui.checkbox.prototype.registerEventListeners = function()
{
    if(this.settings.disabled) return;
    
    var self = this,
        obj = $.extend({},self.settings,{instance: self});

    this.element.on('mouseenter',function(e){
        if( !self.isDisabled() )
            self.ctxMenu.selector.select(self.getID());
            
        if(typeof self.settings.hover === 'function')
            self.settings.hover.call(obj,e);
    });
    this.element.on('mouseleave',function(e){
        self.ctxMenu.selector.deselect();
    });
    
    // Click event
    this.element.click(function(e){
        self.toggle();
        if(typeof self.settings.click === 'function')
        {
            self.settings.click.call(obj,e);
        }
    });
};

ContextMenu.ui.checkbox.prototype.disable = function()
{
    this.element.addClass('contextmenu-disabled');
};

ContextMenu.ui.checkbox.prototype.isDisabled = function()
{
    return this.element.hasClass('contextmenu-disabled');
};


ContextMenu.ui.checkbox.prototype.check = function()
{
    this.element.addClass('contextmenu-checked');
};

ContextMenu.ui.checkbox.prototype.uncheck = function()
{
    this.element.removeClass('contextmenu-checked');
};

ContextMenu.ui.checkbox.prototype.toggle = function()
{
    this.element.toggleClass('contextmenu-checked');
};

ContextMenu.ui.checkbox.prototype.isChecked = function()
{
    return this.element.hasClass('contextmenu-checked');
};

ContextMenu.ui.checkbox.prototype.getID = function()
{
    return this.id;
};

/**
 * @see https://learn.jquery.com/jquery-ui/widget-factory/extending-widgets/
 * @see https://jqueryui.com/widget/
 */
$.widget( "custom.contextMenu", {
    // default options
    options: ContextMenu.defaults,
    // the constructor
    _create: function() 
    {
        // Change the element if the 'selector' option was provided
        // The original element is ignored
        if( this.options.selector !== null ) this.element = $(this.options.selector);
        this.contextMenu = new ContextMenu( this.element, this.options );
    },
    // called when created, and later when changing options
    _refresh: function() {this.contextMenu.build();},
    // events bound via _on are removed automatically
    // revert other modifications here
    _destroy: function() {this.contextMenu.destroy();},
    
    /*-------------------------------------*\
     * Plugin Methods
    \*-------------------------------------*/
    
    show: function(e){
        this.contextMenu.show(e);
        return this.element;
    },
    
    hide: function(e){
        this.contextMenu.hide(e);
        return this.element;
    },
    
    refresh: function(e){
        this.contextMenu.hide(e);
        this.contextMenu.show(e);
        return this.element;
    },
    
    isVisible: function(){
        return this.contextMenu.container.isVisible();
    },
    
    /**
     * Return the currently opened menu items. Only works if the menu is visible.
     */
    getItems: function(){
        var ctx = this.contextMenu;
        if( this.isVisible() )
        {
            return ctx.items.getAll();
        }
    },
    
    select: function(itemID){
        var ctx = this.contextMenu;
        if( ctx.container.isVisible() )
        {
            return ctx.selector.select(itemID);
        }
    }
});

});